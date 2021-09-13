using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RasGIS.CoreFunctions
{
    public class RLProcess
    {
        
        //栅格矢量化, 同时生成面集合
        //参数说明：CELLs,RL_GRID 为栅格场参数、实游程链表结构
        //          map, pset 分别为弧段集合、面集合
        //          CompressID 表示矢量化追踪到下一点的过程中是否进行即时压缩
        //         （对于点集生成缓冲不压缩，弧段等生成缓冲需要压缩，因为存在直线段）

        public int CELLs2Vector_RL(ref RASTER_PAR CELLs, ref FLOAT_RL_LINE[] RL_GRID, ref MAPARC buffer, ref PATCHSET pset)
        {
            //[O]  预处理
            Clear_TinyGap(ref RL_GRID, ref CELLs);         //清除小的游程隙
            //[一] 定义变量与初始化
            int SuccessID = 0;
            int i = 0, j = 0, k = 0, aNum = 0, pNo = 0, PatchNo = 0, LINEs = CELLs.LINEs,
              i0 = 0, j0 = 0, PreID = 0, NxtID = 0, Type = 0, SecNo = 0, pn = 0, pn_FullArc = 0, LR = 0, Start = 0;
            bool IsOutBound = true; double S = 0;
            PNT StartPoint = new PNT(), NxtFP = new PNT(); R_PNT NxtP = new R_PNT(), PreP = new R_PNT(), CurP = new R_PNT(), P0 = new R_PNT();
            FLOAT_RL_LinkTable tmpRL;

            Byte[][] V1 = new Byte[CELLs.LINEs][];//所有游程左侧是否被追
            Byte[][] V2 = new Byte[CELLs.LINEs][];//所有游程右侧是否被追
            int[][] V3 = new int[CELLs.LINEs][];////标记每个游程所属的多边形编号
            for (i = 0; i < LINEs; i++) if (RL_GRID[i].rl_Num > 0)
                {
                    i0 = RL_GRID[i].rl_Num / 8 + 1;

                    V1[i] = new Byte[i0];
                    V2[i] = new Byte[i0];
                    for (int kk = 0; kk < i0; kk++)
                    {
                        V1[i][kk] = 255;//某一侧未被追踪，则将其标记为true
                        V2[i][kk] = 255;

                    }
                    V3[i] = new int[RL_GRID[i].rl_Num];


                }
            //表示某一条弧段属于哪一个面，该数组的下标是弧段编号，对应的值则是该弧段所在的多边形
            int[] RelatePatch = new int[ConstVar._MAX_BufferMap_ArcNumber];
            //用于标记该弧段是外边界还是内边界，true是外边界，默认都是外边界
            bool[] ArcSideID = new bool[ConstVar._MAX_BufferMap_ArcNumber];
            for (int kk = 0; kk < ConstVar._MAX_BufferMap_ArcNumber; kk++)
            {
                RelatePatch[kk] = 0;//表示某一条弧段属于哪一个面
                ArcSideID[kk] = true;//表示该弧是内边界(false)还是外边界(true)
            }

            //定义一些临时弧段
            ARC FullArc = new ARC();
            FullArc.pts = new PNT[ConstVar._MAX_TrackArc_PointNumber];
            //定义临时弧段
            ARC Arc = new ARC();
            Arc.pts = new PNT[100];
            ARC Arc2 = new ARC();

            //if (!SingleObjectMode) Init_Map(ref buffer, ConstVar._MAX_BufferMap_ArcNumber);
            //else Init_Map(ref buffer, 1000);
            Init_Map(ref buffer, ConstVar._MAX_BufferMap_ArcNumber);

            pset.PatchNum = 0;
            //[二] 矢量化及建立面-弧拓扑, 本过程中获得的长度单位为栅格单位
            //遍历栅格行
            for (i = 0; i < LINEs; i++)
            {
                //遍历每个栅格行上的游程
                for (j = 0; j < RL_GRID[i].rl_Num; j++) for (LR = 1; LR <= 2; LR++)
                    {

                        //1.首先找到一个起始点
                        Start = -1;
                        if ((LR == 1) && ((V1[i][j / 8] & ConstVar.bits[j % 8 + 1]) != 0)) Start = 1;
                        else if ((LR == 2) && ((V2[i][j / 8] & ConstVar.bits[j % 8 + 1]) != 0) && (V3[i][j] < 65535)) Start = 2;
                        if (Start <= 0) continue;

                        tmpRL = RL_GRID[i].Line_RLs[j];
                        if (Start == 1)
                        {
                            CurP.x = P0.x = PreP.x = (int)(tmpRL.C1);
                            Arc.pts[0].x = tmpRL.C1; V1[i][j / 8] &= (Byte)~ConstVar.bits[j % 8 + 1]; IsOutBound = true;
                            V3[i][j] = pset.PatchNum;
                        }
                        else if (Start == 2)
                        {
                            CurP.x = P0.x = PreP.x = (int)(tmpRL.C2) + 1;
                            Arc.pts[0].x = tmpRL.C2; V2[i][j / 8] &= (Byte)~ConstVar.bits[j % 8 + 1]; IsOutBound = false;
                            PatchNo = V3[i][j];
                        }
                        Arc.pts[0].y = i + 0.5; P0.y = PreP.y = i; CurP.y = i + 1;
                        pn = 1; pn_FullArc = 0;
                        StartPoint = Arc.pts[0];
                        //2.从起始点开始追踪一条闭合的弧段
                        for (; ; )
                        {
                            i0 = CurP.x - 1; j0 = CurP.y - 1;
                            //2.1根据当前点与前一点的关系，获得下一点的走向
                            PreID = (PreP.x < CurP.x) ? 1 : ((PreP.x > CurP.x) ? 2 : ((PreP.y < CurP.y) ? 3 : 4));
                            switch (PreID)
                            {
                                case 1:
                                    if (GridValue(ref RL_GRID, i0, j0)) { if (GridValue(ref RL_GRID, i0 + 1, j0)) NxtID = (GridValue(ref RL_GRID, i0 + 1, j0 + 1)) ? 4 : 2; else NxtID = 3; }
                                    else { if (GridValue(ref RL_GRID, i0 + 1, j0 + 1)) NxtID = (GridValue(ref RL_GRID, i0 + 1, j0)) ? 3 : 2; else NxtID = 4; }
                                    break;
                                case 2:
                                    if (GridValue(ref RL_GRID, i0 + 1, j0)) { if (GridValue(ref RL_GRID, i0, j0)) NxtID = (GridValue(ref RL_GRID, i0, j0 + 1)) ? 4 : 1; else NxtID = 3; }
                                    else { if (GridValue(ref RL_GRID, i0, j0 + 1)) NxtID = (GridValue(ref RL_GRID, i0, j0)) ? 3 : 1; else NxtID = 4; }
                                    break;
                                case 3:
                                    if (GridValue(ref RL_GRID, i0, j0)) { if (GridValue(ref RL_GRID, i0, j0 + 1)) NxtID = (GridValue(ref RL_GRID, i0 + 1, j0 + 1)) ? 2 : 4; else NxtID = 1; }
                                    else { if (GridValue(ref RL_GRID, i0 + 1, j0 + 1)) NxtID = (GridValue(ref RL_GRID, i0, j0 + 1)) ? 1 : 4; else NxtID = 2; }
                                    break;
                                case 4:
                                    if (GridValue(ref RL_GRID, i0, j0 + 1)) { if (GridValue(ref RL_GRID, i0, j0)) NxtID = (GridValue(ref RL_GRID, i0 + 1, j0)) ? 2 : 3; else NxtID = 1; }
                                    else { if (GridValue(ref RL_GRID, i0 + 1, j0)) NxtID = (GridValue(ref RL_GRID, i0, j0)) ? 1 : 3; else NxtID = 2; }
                                    break;
                            }
                            //2.2 根据下一点的走向获得下一点坐标
                            switch (NxtID)
                            {
                                case 1: NxtP.x = CurP.x - 1; NxtP.y = CurP.y; break; //左
                                case 2: NxtP.x = CurP.x + 1; NxtP.y = CurP.y; break; //右
                                case 3:
                                    NxtP.x = CurP.x; NxtP.y = CurP.y - 1;             //下
                                    if (IsOnSECs(0, RL_GRID[NxtP.y].rl_Num - 1, NxtP.x - 1, NxtP.y, ref RL_GRID, ref Type, ref SecNo))
                                    {
                                        if (Type == 1)
                                        {
                                            V1[NxtP.y][SecNo / 8] &= (Byte)~ConstVar.bits[SecNo % 8 + 1];
                                            NxtFP.x = RL_GRID[NxtP.y].Line_RLs[SecNo].C1;
                                        }
                                        else
                                        {
                                            V2[NxtP.y][SecNo / 8] &= (Byte)~ConstVar.bits[SecNo % 8 + 1];
                                            NxtFP.x = RL_GRID[NxtP.y].Line_RLs[SecNo].C2;
                                        }
                                        NxtFP.y = NxtP.y + 0.5;
                                        V3[NxtP.y][SecNo] = (IsOutBound) ? pset.PatchNum : PatchNo;
                                    }
                                    break;
                                case 4:
                                    NxtP.x = CurP.x; NxtP.y = CurP.y + 1;             //上
                                    if (IsOnSECs(0, RL_GRID[NxtP.y - 1].rl_Num - 1, NxtP.x - 1, NxtP.y - 1, ref RL_GRID, ref Type, ref SecNo))
                                    {
                                        if (Type == 1)
                                        {
                                            V1[NxtP.y - 1][SecNo / 8] &= (Byte)~ConstVar.bits[SecNo % 8 + 1];
                                            NxtFP.x = RL_GRID[NxtP.y - 1].Line_RLs[SecNo].C1;
                                        }
                                        else
                                        {
                                            V2[NxtP.y - 1][SecNo / 8] &= (Byte)~ConstVar.bits[SecNo % 8 + 1];
                                            NxtFP.x = RL_GRID[NxtP.y - 1].Line_RLs[SecNo].C2;
                                        }
                                        NxtFP.y = NxtP.y - 1 + 0.5;
                                        V3[NxtP.y - 1][SecNo] = (IsOutBound) ? pset.PatchNum : PatchNo;
                                    }
                                    break;
                            }
                            //2.3 进行弧段压缩
                            if (NxtID >= 3)
                            {
                                if (pn >= 2)
                                {
                                    if (Math.Abs((Arc.pts[pn - 1].x - NxtFP.x) * (Arc.pts[pn - 2].y - Arc.pts[pn - 1].y) - (Arc.pts[pn - 2].x - Arc.pts[pn - 1].x) * (Arc.pts[pn - 1].y - NxtFP.y)) > 0.04)
                                        Arc.pts[pn++] = NxtFP;
                                    else Arc.pts[pn - 1] = NxtFP;
                                }
                                else Arc.pts[pn++] = NxtFP;
                            }
                            //2.4.1 如果闭合则终止本弧追踪
                            if (NxtP.x == P0.x && NxtP.y == P0.y)
                            {
                                Arc.pts[pn++] = StartPoint;
                                Arc.pNum = pn;
                                ARC_ARJ(ref Arc, ref Arc2, 0.5);
                                for (k = 0; k < Arc2.pNum; k++) FullArc.pts[pn_FullArc++] = Arc2.pts[k];
                                break;
                            }
                            //2.4.2 如果总点数过多则终止本弧追踪
                            if (pn_FullArc >= ConstVar._MAX_TrackArc_PointNumber - 1) { SuccessID = -1; break; }
                            if (pn >= 100)
                            {
                                Arc.pNum = pn;
                                ARC_ARJ(ref Arc, ref Arc2, 0.5);
                                for (k = 0; k < Arc2.pNum; k++) FullArc.pts[pn_FullArc++] = Arc2.pts[k];
                                pn = 0;
                            }
                            //2.5 循环重置——从下一点开始追踪下下点
                            PreP = CurP;
                            CurP = NxtP;

                        }
                        if (SuccessID == -1) break;
                        FullArc.pNum = pn_FullArc;
                        if (aNum >= ConstVar._MAX_BufferMap_ArcNumber - 1) { SuccessID = -2; break; }
                        // 3.对上面追踪出来的弧段进行处理: 过小则舍弃；设置该弧段在关联面上的属性（内边界、外边界）
                        if (pn_FullArc <= 10)
                        {
                            for (pNo = 0, S = 0; pNo < pn_FullArc; pNo++)
                            {
                                if (pNo == pn_FullArc - 1)
                                    S += 0.5 * (FullArc.pts[pNo].y + FullArc.pts[0].y)
                                   * (FullArc.pts[0].x - FullArc.pts[pNo].x);
                                else S += 0.5 * (FullArc.pts[pNo].y + FullArc.pts[pNo + 1].y)
                                   * (FullArc.pts[pNo + 1].x - FullArc.pts[pNo].x);
                            }
                        }
                        if (pn_FullArc > 10 || (pn_FullArc <= 10 && Math.Abs(S) > 2))
                        {
                            //将当前追踪并经过分段压缩处理的完整弧段FullArc存储到缓冲图层
                            ARC_ARJ(ref FullArc, ref buffer.arcs[aNum++], -1);
                            //设置弧段的面关联属性(所属面，是外还是内边界)
                            if (IsOutBound) RelatePatch[aNum - 1] = pset.PatchNum++;
                            else { ArcSideID[aNum - 1] = false; RelatePatch[aNum - 1] = PatchNo; }
                        }
                    }



            }
            //所有行上的游程都遍历完
            if (SuccessID == 0)
            {
                //[三] 上面记录的坐标变换到地图坐标系中
                buffer.aNum = aNum;
                double xmin = CELLs.xmin, ymin = CELLs.ymin, dx = CELLs.dxy, dy = CELLs.dxy;
                for (i = 0; i < aNum; i++) for (j = 0; j < buffer.arcs[i].pNum; j++)
                    {
                        buffer.arcs[i].pts[j].x = buffer.arcs[i].pts[j].x * dx + xmin;
                        buffer.arcs[i].pts[j].y = buffer.arcs[i].pts[j].y * dy + ymin;
                    }
                //[四] 建立面与弧段之间的关系（前面已获得弧段的从属面等信息，但需按面组织）
                pset.Patchs = new PATCH[pset.PatchNum];
                for (int ii = 0; ii < pset.PatchNum; ii++)
                {
                    pset.Patchs[ii].oNum = 1;
                    pset.Patchs[ii].iNum = 0;
                }
                for (int ii = 0; ii < aNum; ii++)
                {
                    if (ArcSideID[ii]) pset.Patchs[RelatePatch[ii]].oNo = ii;
                    else pset.Patchs[RelatePatch[ii]].iNum++;
                }
                for (int ii = 0; ii < pset.PatchNum; ii++) if (pset.Patchs[ii].iNum > 0)
                        pset.Patchs[ii].iNo = new int[pset.Patchs[ii].iNum];
                for (int ii = 0; ii < pset.PatchNum; ii++) pset.Patchs[ii].iNum = 0;
                for (int ii = 0; ii < aNum; ii++) if (ArcSideID[ii] == false)
                    {
                        PatchNo = RelatePatch[ii];
                        pset.Patchs[PatchNo].iNo[pset.Patchs[PatchNo].iNum++] = ii;
                    }
            }
            else pset.PatchNum = 0;
            return SuccessID;
        }

        //清除小的游程隙
        //前一游程的尾与后一游程的头在同一栅格内可合并
        void Clear_TinyGap(ref FLOAT_RL_LINE[] RL_GRID, ref RASTER_PAR CELLs)
        {
            for (int ii = 0; ii < CELLs.LINEs; ii++) if (RL_GRID[ii].rl_Num >= 2)
                {
                    for (int j = 0; j < RL_GRID[ii].rl_Num - 1; j++)
                    {
                        if ((int)(RL_GRID[ii].Line_RLs[j + 1].C1) - (int)(RL_GRID[ii].Line_RLs[j].C2) <= 1)
                        {
                            RL_GRID[ii].Line_RLs[j].C2 = RL_GRID[ii].Line_RLs[j + 1].C2;
                            RL_GRID[ii].Line_RLs.RemoveAt(j + 1);

                            RL_GRID[ii].rl_Num -= 1; j -= 1;
                        }

                    }
                }
        }

        void Init_Map(ref MAPARC map, int aNum)
        //清空图层，并分配好aNum条弧段的指针
        {
            if (map.aNum > 0)
            {
                for (int i = 0; i < map.aNum; i++)
                {
                    map.arcs[i].pNum = 0;

                    map.arcs[i].pts = null;

                }
                map.arcs = null;
            }

            map.arcs = new ARC[aNum];
            map.aNum = 0;
        }
        //---------------------------------------------------------------------------
        bool GridValue(ref FLOAT_RL_LINE[] RL_GRID, int C, int L)
        //判断在实游程链表格式记录的栅格数据场上，L行、C列的值是true/false?
        {
            if (RL_GRID[L].rl_Num <= 0) return false;
            int i, i1 = 0, i2 = RL_GRID[L].rl_Num - 1;
            if (C > (int)(RL_GRID[L].Line_RLs[i2].C2)) return false;

            for (; ; )
            {
                if (C < (int)(RL_GRID[L].Line_RLs[i1].C1)) return false;
                else
                {
                    if (i1 == i2) return true;
                    else
                    {
                        i = (i1 + i2) / 2;
                        if (C <= (int)(RL_GRID[L].Line_RLs[i].C2)) i2 = i;
                        else i1 = i + 1;
                    }
                }
            }
        }

        //---------------------------------------------------------------------------
        bool IsOnSECs(int i1, int i2, int C, int L, ref FLOAT_RL_LINE[] RL_GRID, ref int Type, ref int SecNo)
        //判断 行L、列C的栅格，在栅格场上处于对应行链表的那一段边界上？如果是，返回true；
        //而具体在哪一段，由参数SecNo返回；
        //Type = 1表示 在左边界上，Type = 2 表示在由边界上
        //此外，i1,i2表示该行的段序号（从、到）范围，并不一定限制为"0~段数-1"，可自定义
        {
            if (C > (int)(RL_GRID[L].Line_RLs[i2].C2)) return false;
            for (; ; )
            {
                if (C < (int)(RL_GRID[L].Line_RLs[i1].C1 - 1)) return false;
                else
                {
                    if (i1 == i2)
                    {
                        if (C == (int)(RL_GRID[L].Line_RLs[i1].C1 - 1)) { Type = 1; SecNo = i1; return true; }
                        if (C == (int)(RL_GRID[L].Line_RLs[i1].C2)) { Type = 2; SecNo = i1; return true; }
                        return false;
                    }
                    else
                    {
                        int i = (i1 + i2) / 2;
                        if (C <= (int)(RL_GRID[L].Line_RLs[i].C2)) i2 = i;
                        else i1 = i + 1;
                    }
                }
            }
        }
        //---------------------------------------------------------------------------
        void ARC_ARJ(ref ARC Arc1, ref ARC Arc2, double Tol)
        //弧段压缩主调函数：Arc1为原始弧段，Arc2为压缩有的新弧段，Tol为压缩参数（径距限值）
        //注：如果Tol <= 0, 则直接拷贝
        {
            if (Arc1.pNum <= 0) return;
            if (Tol < 0 || Arc1.pNum <= 2)
            {
                Arc2.pNum = Arc1.pNum;
                Arc2.pts = new PNT[Arc2.pNum];
                for (int i = 0; i < Arc2.pNum; i++) Arc2.pts[i] = Arc1.pts[i];
                return;
            }
            bool[] SelectID = new bool[Arc1.pNum];
            for (int i = 0; i < Arc1.pNum; i++)
            {
                SelectID[i] = false;
            }

            int Mid = Arc1.pNum / 2; int SelectNum = 3;
            SelectID[0] = SelectID[Arc1.pNum - 1] = SelectID[Mid] = true;
            Compress(Arc1.pts, 0, Mid, Tol * Tol, SelectID, ref SelectNum);
            Compress(Arc1.pts, Mid, Arc1.pNum - 1, Tol * Tol, SelectID, ref SelectNum);
            Arc2.pNum = SelectNum;
            Arc2.pts = new PNT[Arc2.pNum];
            SelectNum = 0;
            for (int i = 0; i < Arc1.pNum; i++)
                if (SelectID[i]) Arc2.pts[SelectNum++] = Arc1.pts[i];
            SelectID = null;
        }
        void Compress(PNT[] pts, int P, int Q, double Tol2, bool[] SelectID, ref int SelectNum)
        //弧段压缩递归过程
        //表示压缩弧段pts(点集)中第P点到第Q点，Tol2为径距限值的平方（主要考虑距离判定不用开方），
        //SelectID[]表示某点被保留或舍弃，
        //SelectNum表示压缩过程中被保留的点数,每保留一点该值加1
        {
            if (Q - P < 2) return;
            double Max = 0, Dpq, A, B, C, dd;
            Dpq = (pts[P].y - pts[Q].y) * (pts[P].y - pts[Q].y)
               + (pts[P].x - pts[Q].x) * (pts[P].x - pts[Q].x);
            A = pts[P].y - pts[Q].y;
            B = pts[Q].x - pts[P].x;
            C = pts[P].x * pts[Q].y - pts[Q].x * pts[P].y;
            int M = -1;
            for (int i = P + 1; i <= Q - 1; i++)
            {
                dd = A * pts[i].x + B * pts[i].y + C;
                if (dd * dd > Max * Dpq) { Max = dd * dd / Dpq; M = i; }
            }
            if (Max > Tol2 && M != -1)
            {
                SelectID[M] = true;
                SelectNum += 1;
                Compress(pts, P, M, Tol2, SelectID, ref SelectNum);
                Compress(pts, M, Q, Tol2, SelectID, ref SelectNum);
            }
            return;
        }
    }


    public static class ConstVar
    {
        public const int _MAX_TrackArc_PointNumber = 10000000;
        //进行栅格数据矢量化产生闭合弧段时，一条弧最多点数
        public const int _MAX_BufferMap_ArcNumber = 100000;
        //生成的缓冲区图层上弧段总数上限
        public const int _MAX_RASTER_LCs = 100000;
        public const int _MIN_RASTER_LCs = 5000;
        //缓冲计算栅格场的最大、最小行数或列数
        public const int _MAX_RL_STACK = 5000;
        //游程堆栈最大值
        public const int _MAX_PNUM_anArc = 100000;
        //一条弧的最多点数(与矢量化中追踪弧的最多点数相同)
        public const int _MAX_ARCNUM_aMap = 100000;
        //一幅地图的最多弧数(与矢量化缓冲区图层最多弧数相同)
        public const int _DEFAULT_R2CELLSIZE = 100;
        //缺省的格网大小(缓冲距离的1/100)
        public const int _LEFT = 1;
        public const int _RIGHT = 2;
        public const int _UP = 3;
        public const int _DOWN = 4;
        public const double _PI = 3.14159265358979323846;
        public const double _PI2 = 6.28318530717958647692;
        public const double _PI_180 = 0.017453292519943295769;
        public const int _xMAX_RASTER_LCs = 5000;
        //栅格场最大行数
        public const int _xMIN_RASTER_LCs = 100;
        //栅格场最小行数
        public const int _MAXPNUM_ARCSEC = 10;
        //一条弧的点集分组时各组内的最大点数

        public static Byte[] bits = new Byte[9] { 0x00, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
        //在判断某个字节的第 1-8 位分别为 0或1 时需用到以上模板


        public const int ZOOMIN = 10001;   //放大
        public const int ZOOMOUT = 10002;  //缩小
        public const int PAN = 10003;   //移动
        public const int SELECT = 10004;  //选择
        public const double TOL = 8.0;     //屏幕上选取目标的距离容差
        public const double ZoomFactor = 2.0;

        public const int WIDTH_WORK = 1;
        public const int WIDTH_BACK = 1;
        public const int WIDTH_CALC = 2;
        public const int WIDTH_SELECT = 2;
    }
}
