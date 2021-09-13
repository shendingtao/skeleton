using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Text;
namespace RasGIS.CoreFunctions
{
    class CreateCenterLines
    {
        //用游程编码解决数据量太大的问题
        public void CenterLineCompt2(string CenterLineDir,string linedie, ref string Message)
        {
            StreamReader sr = new StreamReader(CenterLineDir);
            //////////////读水域面文件////////////////////

            MAPARC riverbuffer = new MAPARC();
            PATCHSET riverpset = new PATCHSET();
            //分析多边形数目
            string readline = sr.ReadLine();
            string[] splitline = readline.Split(',');
            riverpset.PatchNum =Convert.ToInt32(splitline[1]);
            riverpset.Patchs = new PATCH[riverpset.PatchNum];

            riverbuffer.aNum = ConstVar._MAX_ARCNUM_aMap;
            riverbuffer.arcs = new ARC[riverbuffer.aNum];
            int PatchNo = -1;
            int ArcNo = -1;

            string Flag = string.Empty;
            int pointindex = 0;
            int inindex = -1;
            for (; ; )
            {
                readline = sr.ReadLine();
                if (readline.Trim() == "END") break;
                
                splitline = readline.Split(',');
                if (splitline[0] == "OUT")//多边形外环
                {
                    Flag = "OUT";
                    pointindex = 0;
                    inindex = -1;
                    ArcNo++;
                    PatchNo++;
                    int pntnum = Convert.ToInt32(splitline[1]);
                    riverbuffer.arcs[ArcNo].pNum = pntnum;
                    riverbuffer.arcs[ArcNo].pts = new PNT[riverbuffer.arcs[ArcNo].pNum];
                    riverpset.Patchs[PatchNo].oNum = 1;
                    riverpset.Patchs[PatchNo].oNo = ArcNo;
                    
                    continue;
                }
                else if(splitline[0] == "IN")
                {
                    Flag = "IN";
                    
                    riverpset.Patchs[PatchNo].iNum =Convert.ToInt32(splitline[1]);
                    riverpset.Patchs[PatchNo].iNo = new int[riverpset.Patchs[PatchNo].iNum];
                    
                }
                else if (splitline[0] == "INO")
                {
                    inindex++;
                    ArcNo++;
                    pointindex = 0;
                    int pntnum = Convert.ToInt32(splitline[1]);
                    riverbuffer.arcs[ArcNo].pNum = pntnum;
                    riverbuffer.arcs[ArcNo].pts = new PNT[riverbuffer.arcs[ArcNo].pNum];
                    riverpset.Patchs[PatchNo].iNo[inindex] = ArcNo;

                }
                else
                {
                    if (Flag == "OUT")
                    {
                        riverbuffer.arcs[ArcNo].pts[pointindex].x = Convert.ToDouble(splitline[0]);
                        riverbuffer.arcs[ArcNo].pts[pointindex].y = Convert.ToDouble(splitline[1]);
                        pointindex++;
                    }
                    else if(Flag == "IN")
                    {
                        
                        riverbuffer.arcs[ArcNo].pts[pointindex].x = Convert.ToDouble(splitline[0]);
                        riverbuffer.arcs[ArcNo].pts[pointindex].y = Convert.ToDouble(splitline[1]);
                        pointindex++;
                    }
                }
            }
            

            
            

            




            

            //独立遍历每个多边形
            MAPARC[] CentetLines = new MAPARC[riverpset.PatchNum];  //存储中心线结果


            //逐个多边形进行遍历
            

            double size =1;

            for (int ii = 0; ii < riverpset.PatchNum; ii++)
            //for (int ii = 0; ii < 1; ii++)
            {

               
                CentetLines[ii].aNum = 0;
                CentetLines[ii].arcs = new ARC[100000];

                MAPARC tmpbuffer = new MAPARC();//
                tmpbuffer.Tol = size;

                PATCHSET tmpset = new PATCHSET();

                //将每个面依次作为一个图层输入
                //线的数量等于外环+内环
                tmpbuffer.aNum = riverpset.Patchs[ii].oNum + riverpset.Patchs[ii].iNum;
                tmpbuffer.arcs = new ARC[tmpbuffer.aNum];
                tmpset.PatchNum = 1;
                tmpset.Patchs = new PATCH[tmpset.PatchNum];
                tmpset.Patchs[0].oNum = riverpset.Patchs[ii].oNum;
                tmpset.Patchs[0].oNo = riverpset.Patchs[ii].oNo;
                tmpset.Patchs[0].iNum = riverpset.Patchs[ii].iNum;
                tmpset.Patchs[0].iNo = new int[tmpset.Patchs[0].iNum];

                tmpbuffer.arcs[0].pNum = riverbuffer.arcs[riverpset.Patchs[ii].oNo].pNum;
                tmpbuffer.arcs[0].pts = new PNT[tmpbuffer.arcs[0].pNum];
                for (int jj = 0; jj < tmpbuffer.arcs[0].pNum; jj++)
                {
                    tmpbuffer.arcs[0].pts[jj].x = riverbuffer.arcs[riverpset.Patchs[ii].oNo].pts[jj].x;
                    tmpbuffer.arcs[0].pts[jj].y = riverbuffer.arcs[riverpset.Patchs[ii].oNo].pts[jj].y;
                }

                for (int jj = 0; jj < riverpset.Patchs[ii].iNum; jj++)
                {
                    tmpset.Patchs[0].iNo[jj] = riverpset.Patchs[ii].iNo[jj];
                    tmpbuffer.arcs[jj + 1].pNum = riverbuffer.arcs[riverpset.Patchs[ii].iNo[jj]].pNum;
                    tmpbuffer.arcs[jj + 1].pts = new PNT[tmpbuffer.arcs[jj + 1].pNum];

                    for (int kk = 0; kk < riverbuffer.arcs[riverpset.Patchs[ii].iNo[jj]].pNum; kk++)
                    {
                        tmpbuffer.arcs[jj + 1].pts[kk].x = riverbuffer.arcs[riverpset.Patchs[ii].iNo[jj]].pts[kk].x;
                        tmpbuffer.arcs[jj + 1].pts[kk].y = riverbuffer.arcs[riverpset.Patchs[ii].iNo[jj]].pts[kk].y;
                    }
                }

                MapBoundInitial(ref tmpbuffer);



                tmpset.PatchNum = 1;
                tmpset.Patchs = new PATCH[tmpset.PatchNum];
                tmpset.RemoveSELF = false;
                tmpset.Patchs[0].oNum = 1;
                tmpset.Patchs[0].oNo = 0;
                tmpset.Patchs[0].iNum = tmpbuffer.aNum - 1;
                tmpset.Patchs[0].iNo = new int[tmpset.Patchs[0].iNum];
                for (int jj = 1; jj < tmpbuffer.aNum; jj++)
                {
                    tmpset.Patchs[0].iNo[jj - 1] = jj;
                }

                RASTER_PAR CELLs0 = new RASTER_PAR();
                GetRasterPara_BufARCS(ref tmpbuffer, ref CELLs0);

                //1. 栅格化
                FLOAT_RL_LINE[] RL_GRID0 = new FLOAT_RL_LINE[CELLs0.LINEs];
                for (int i = 0; i < CELLs0.LINEs; i++)
                {
                    RL_GRID0[i] = new FLOAT_RL_LINE();
                    RL_GRID0[i].rl_Num = 0;
                    RL_GRID0[i].Line_RLs = new List<FLOAT_RL_LinkTable>();
                }

                PatchSet_To_RLTable(ref tmpbuffer, ref tmpset, ref RL_GRID0, ref CELLs0, true);

                //---------------------------核心函数-----------------------
                long curmem = 0;
                for (int i = 0; i < CELLs0.LINEs; i++)
                {
                    if (RL_GRID0[i].rl_Num != 0) curmem += 8 * RL_GRID0[i].rl_Num;
                }

                

                int loop = 1;
                bool Changed = false;

                for (; ; )
                {
                    //-----------------------------------------------------一次扫描
                    int j = 0, k = 0, LINEs = CELLs0.LINEs,
                      i0 = 0, j0 = 0, PreID = 0, Type = 0, SecNo = 0, NxtID = 0;

                    R_PNT NxtP = new R_PNT(), PreP = new R_PNT(), CurP = new R_PNT(), P0 = new R_PNT(), InsertP = new R_PNT();
                    FLOAT_RL_LinkTable tmpRL;

                    //[二] 矢量化及建立面-弧拓扑, 本过程中获得的长度单位为栅格单位
                    //遍历栅格行

                    FLOAT_R_LINE[] R_GRID = new FLOAT_R_LINE[CELLs0.LINEs];
                    Byte[][] V1 = new Byte[CELLs0.LINEs][];//所有游程左侧是否被追
                    Byte[][] V2 = new Byte[CELLs0.LINEs][];//所有游程右侧是否被追
                    for (int t = 0; t < CELLs0.LINEs; t++)
                    {
                        R_GRID[t] = new FLOAT_R_LINE();
                        R_GRID[t].rl_Num = 0;
                        R_GRID[t].Line_Rs = new List<FLOAT_R_LinkTable>();

                        i0 = RL_GRID0[t].rl_Num / 8 + 1;

                        V1[t] = new Byte[i0];
                        V2[t] = new Byte[i0];
                        for (int kk = 0; kk < i0; kk++)
                        {
                            V1[t][kk] = 255;//某一侧未被追踪，则将其标记为true
                            V2[t][kk] = 255;

                        }
                    }
                    for (int i = 0; i < LINEs; i++)
                    {
                        //遍历每个栅格行上的游程
                        for (j = 0; j < RL_GRID0[i].rl_Num; j++) for (int LR = 1; LR <= 2; LR++)
                            {

                                //1.首先找到一个起始点
                                int Start = -1;
                                if ((LR == 1) && ((V1[i][j / 8] & ConstVar.bits[j % 8 + 1]) != 0)) Start = 1;
                                else if ((LR == 2) && ((V2[i][j / 8] & ConstVar.bits[j % 8 + 1]) != 0)) Start = 2;
                                if (Start <= 0) continue;

                                tmpRL = RL_GRID0[i].Line_RLs[j];
                                if (Start == 1)
                                {
                                    CurP.x = P0.x = PreP.x = (int)(tmpRL.C1);
                                    V1[i][j / 8] &= (Byte)~ConstVar.bits[j % 8 + 1];

                                    InsertP.x = PreP.x;

                                }
                                else if (Start == 2)
                                {
                                    CurP.x = P0.x = PreP.x = (int)(tmpRL.C2) + 1;
                                    V2[i][j / 8] &= (Byte)~ConstVar.bits[j % 8 + 1];

                                    InsertP.x = PreP.x - 1;
                                }
                                P0.y = PreP.y = i; CurP.y = i + 1;
                                InsertP.y = PreP.y;
                                //看当前点是不是可以剥离的点
                                //计算InsertP这个点的连通性
                                if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;


                                //2.从起始点开始追踪一条闭合的弧段
                                for (; ; )
                                {
                                    i0 = CurP.x - 1; j0 = CurP.y - 1;
                                    //2.1根据当前点与前一点的关系，获得下一点的走向
                                    PreID = (PreP.x < CurP.x) ? 1 : ((PreP.x > CurP.x) ? 2 : ((PreP.y < CurP.y) ? 3 : 4));
                                    switch (PreID)
                                    {
                                        case 1:
                                            if (GridValue(ref RL_GRID0, i0, j0)) //外边
                                            {
                                                if (GridValue(ref RL_GRID0, i0 + 1, j0))
                                                {
                                                    if (GridValue(ref RL_GRID0, i0 + 1, j0 + 1))//向右向上
                                                    {
                                                        NxtID = 4;
                                                        InsertP.x++;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                        InsertP.y++;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    else//向右向右
                                                    {
                                                        NxtID = 2;
                                                        InsertP.x++;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    //NxtID = (GridValue(ref RL_GRID0, i0 + 1, j0 + 1)) ? 4 : 2; 
                                                }
                                                else//向右向下
                                                {
                                                    NxtID = 3;
                                                }
                                            }
                                            else //内边
                                            {
                                                if (GridValue(ref RL_GRID0, i0 + 1, j0 + 1))
                                                {
                                                    if (GridValue(ref RL_GRID0, i0 + 1, j0))//向右向下
                                                    {
                                                        NxtID = 3;
                                                        InsertP.x++;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                        InsertP.y--;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    else//向右向右
                                                    {
                                                        NxtID = 2;
                                                        InsertP.x++;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    //NxtID = (GridValue(ref RL_GRID0, i0 + 1, j0)) ? 3 : 2;
                                                }
                                                else NxtID = 4;//向右向上 
                                            }
                                            break;
                                        case 2:
                                            if (GridValue(ref RL_GRID0, i0 + 1, j0)) //内边
                                            {
                                                if (GridValue(ref RL_GRID0, i0, j0))
                                                {
                                                    if (GridValue(ref RL_GRID0, i0, j0 + 1))//向左向上
                                                    {
                                                        NxtID = 4;
                                                        InsertP.x--;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                        InsertP.y++;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    else //向左向左
                                                    {
                                                        NxtID = 1;
                                                        InsertP.x--;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    //NxtID = (GridValue(ref RL_GRID0, i0, j0 + 1)) ? 4 : 1;
                                                }
                                                else NxtID = 3; //向左向下
                                            }
                                            else //外边
                                            {
                                                if (GridValue(ref RL_GRID0, i0, j0 + 1))
                                                {
                                                    if (GridValue(ref RL_GRID0, i0, j0))//向左向下
                                                    {
                                                        NxtID = 3;
                                                        InsertP.x--;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                        InsertP.y--;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;

                                                    }
                                                    else//向左向左
                                                    {
                                                        NxtID = 1;
                                                        InsertP.x--;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    //NxtID = (GridValue(ref RL_GRID0, i0, j0)) ? 3 : 1;
                                                }
                                                else
                                                {
                                                    NxtID = 4;//向左向上
                                                }
                                            }
                                            break;
                                        case 3://向上
                                            if (GridValue(ref RL_GRID0, i0, j0))  //内边
                                            {
                                                if (GridValue(ref RL_GRID0, i0, j0 + 1))
                                                {
                                                    if (GridValue(ref RL_GRID0, i0 + 1, j0 + 1))//向上向右
                                                    {
                                                        NxtID = 2;
                                                        InsertP.y++;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                        InsertP.x++;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    else//向上向上
                                                    {
                                                        NxtID = 4;
                                                        InsertP.y++;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    //NxtID = (GridValue(ref RL_GRID0, i0 + 1, j0 + 1)) ? 2 : 4;
                                                }
                                                else NxtID = 1; //向上向左
                                            }
                                            else  //外边
                                            {
                                                if (GridValue(ref RL_GRID0, i0 + 1, j0 + 1))
                                                {
                                                    if (GridValue(ref RL_GRID0, i0, j0 + 1))
                                                    {
                                                        NxtID = 1;//从上往左
                                                        InsertP.y++;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                        InsertP.x--;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    else
                                                    {
                                                        NxtID = 4;//从上往上
                                                        InsertP.y++;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    //NxtID = (GridValue(ref RL_GRID0, i0, j0 + 1)) ? 1 : 4;
                                                }
                                                else
                                                {
                                                    NxtID = 2;//从上向右，插入点不变
                                                }
                                            }
                                            break;
                                        case 4://向下
                                            if (GridValue(ref RL_GRID0, i0, j0 + 1)) //外边
                                            {
                                                if (GridValue(ref RL_GRID0, i0, j0))
                                                {
                                                    if (GridValue(ref RL_GRID0, i0 + 1, j0))//向下向右
                                                    {
                                                        NxtID = 2;
                                                        InsertP.y--;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                        InsertP.x++;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    else//向下向下
                                                    {
                                                        NxtID = 3;
                                                        InsertP.y--;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    //NxtID = (GridValue(ref RL_GRID0, i0 + 1, j0)) ? 2 : 3;
                                                }
                                                else NxtID = 1; //向下向左
                                            }
                                            else
                                            {
                                                if (GridValue(ref RL_GRID0, i0 + 1, j0))
                                                {
                                                    if (GridValue(ref RL_GRID0, i0, j0))//向下向左
                                                    {
                                                        NxtID = 1;
                                                        InsertP.y--;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                        InsertP.x--;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    else//向下向下
                                                    {
                                                        NxtID = 3;
                                                        InsertP.y--;
                                                        if (InsertPointL(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    //NxtID = (GridValue(ref RL_GRID0, i0, j0)) ? 1 : 3;
                                                }
                                                else NxtID = 2; //向下向右
                                            }
                                            break;
                                    }
                                    //2.2 根据下一点的走向获得下一点坐标

                                    switch (NxtID)
                                    {
                                        case 1:
                                            NxtP.x = CurP.x - 1; NxtP.y = CurP.y;

                                            break; //左
                                        case 2:
                                            NxtP.x = CurP.x + 1; NxtP.y = CurP.y;

                                            break; //右
                                        case 3:
                                            NxtP.x = CurP.x; NxtP.y = CurP.y - 1;             //下
                                            if (IsOnSECs(0, RL_GRID0[NxtP.y].rl_Num - 1, NxtP.x - 1, NxtP.y, ref RL_GRID0, ref Type, ref SecNo))
                                            {
                                                if (Type == 1)
                                                {
                                                    V1[NxtP.y][SecNo / 8] &= (Byte)~ConstVar.bits[SecNo % 8 + 1];

                                                }
                                                else if (Type == 2)
                                                {
                                                    V2[NxtP.y][SecNo / 8] &= (Byte)~ConstVar.bits[SecNo % 8 + 1];
                                                }
                                                else //type等于3  此时游程只有一个网格
                                                {
                                                    V1[NxtP.y][SecNo / 8] &= (Byte)~ConstVar.bits[SecNo % 8 + 1];
                                                    V2[NxtP.y][SecNo / 8] &= (Byte)~ConstVar.bits[SecNo % 8 + 1];
                                                }

                                            }
                                            break;
                                        case 4:
                                            NxtP.x = CurP.x; NxtP.y = CurP.y + 1;             //上

                                            if (IsOnSECs(0, RL_GRID0[NxtP.y - 1].rl_Num - 1, NxtP.x - 1, NxtP.y - 1, ref RL_GRID0, ref Type, ref SecNo))
                                            {
                                                if (Type == 1)
                                                {
                                                    V1[NxtP.y - 1][SecNo / 8] &= (Byte)~ConstVar.bits[SecNo % 8 + 1];

                                                }
                                                else
                                                {
                                                    V2[NxtP.y - 1][SecNo / 8] &= (Byte)~ConstVar.bits[SecNo % 8 + 1];

                                                }



                                            }

                                            break;
                                    }
                                    //2.4.1 如果闭合则终止本弧追踪
                                    if (NxtP.x == P0.x && NxtP.y == P0.y)
                                    {
                                        break;
                                    }


                                    //2.5 循环重置——从下一点开始追踪下下点
                                    PreP = CurP;
                                    CurP = NxtP;

                                }
                            }
                    }
                    //所有行上的游程都遍历完，剥离左半边点
                    for (int i = 0; i < CELLs0.LINEs; i++)
                    {
                        int C1 = -1;
                        int C2 = -1;


                        for (int jjj = 0; jjj < R_GRID[i].rl_Num; jjj++)
                        {
                            if (jjj == 0)
                            {
                                C1 = (int)R_GRID[i].Line_Rs[jjj].C1;
                                C2 = (int)R_GRID[i].Line_Rs[jjj].C1;
                                if (jjj == (R_GRID[i].rl_Num - 1))
                                {
                                    //C2 = (int)R_GRID[i].Line_Rs[jjj].C1;
                                    //擦除对应游程
                                    RemoveRL_Float(i, C1, C2, ref RL_GRID0);
                                    break;
                                }
                                continue;
                            }
                            else if ((C2 + 1) == (int)R_GRID[i].Line_Rs[jjj].C1)
                            {
                                C2++;
                                if (jjj == (R_GRID[i].rl_Num - 1))
                                {
                                    //C2 = (int)R_GRID[i].Line_Rs[jjj].C1;
                                    //擦除对应游程
                                    RemoveRL_Float(i, C1, C2, ref RL_GRID0);
                                    break;
                                }
                            }
                            else
                            {
                                //擦除对应游程
                                RemoveRL_Float(i, C1, C2, ref RL_GRID0);

                                C1 = (int)R_GRID[i].Line_Rs[jjj].C1;
                                C2 = (int)R_GRID[i].Line_Rs[jjj].C1;
                                if (jjj == (R_GRID[i].rl_Num - 1))
                                {
                                    //C2 = (int)R_GRID[i].Line_Rs[jjj].C1;
                                    //擦除对应游程
                                    RemoveRL_Float(i, C1, C2, ref RL_GRID0);
                                    break;
                                }
                            }


                        }

                    }

                    if (Changed == false)//无法继续剥离
                    {
                        break;
                    }
                    else
                    {
                        Changed = false;
                    }


                    //-----------------------------------------------------二次扫描

                    for (int t = 0; t < CELLs0.LINEs; t++)
                    {
                        R_GRID[t] = new FLOAT_R_LINE();
                        R_GRID[t].rl_Num = 0;
                        R_GRID[t].Line_Rs = new List<FLOAT_R_LinkTable>();

                        i0 = RL_GRID0[t].rl_Num / 8 + 1;

                        V1[t] = new Byte[i0];
                        V2[t] = new Byte[i0];
                        for (int kk = 0; kk < i0; kk++)
                        {
                            V1[t][kk] = 255;//某一侧未被追踪，则将其标记为true
                            V2[t][kk] = 255;

                        }
                    }
                    for (int i = 0; i < LINEs; i++)
                    {


                        //遍历每个栅格行上的游程
                        for (j = 0; j < RL_GRID0[i].rl_Num; j++) for (int LR = 1; LR <= 2; LR++)
                            {

                                //1.首先找到一个起始点
                                int Start = -1;
                                if ((LR == 1) && ((V1[i][j / 8] & ConstVar.bits[j % 8 + 1]) != 0)) Start = 1;
                                else if ((LR == 2) && ((V2[i][j / 8] & ConstVar.bits[j % 8 + 1]) != 0)) Start = 2;
                                if (Start <= 0) continue;

                                tmpRL = RL_GRID0[i].Line_RLs[j];
                                if (Start == 1)
                                {
                                    CurP.x = P0.x = PreP.x = (int)(tmpRL.C1);
                                    V1[i][j / 8] &= (Byte)~ConstVar.bits[j % 8 + 1];

                                    InsertP.x = PreP.x;

                                }
                                else if (Start == 2)
                                {
                                    CurP.x = P0.x = PreP.x = (int)(tmpRL.C2) + 1;
                                    V2[i][j / 8] &= (Byte)~ConstVar.bits[j % 8 + 1];

                                    InsertP.x = PreP.x - 1;
                                }
                                P0.y = PreP.y = i; CurP.y = i + 1;
                                InsertP.y = PreP.y;
                                //看当前点是不是可以剥离的点
                                //计算InsertP这个点的连通性
                                if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;

                                //2.从起始点开始追踪一条闭合的弧段
                                for (; ; )
                                {
                                    i0 = CurP.x - 1; j0 = CurP.y - 1;

                                    //2.1根据当前点与前一点的关系，获得下一点的走向
                                    PreID = (PreP.x < CurP.x) ? 1 : ((PreP.x > CurP.x) ? 2 : ((PreP.y < CurP.y) ? 3 : 4));
                                    switch (PreID)
                                    {
                                        case 1:
                                            if (GridValue(ref RL_GRID0, i0, j0)) //外边
                                            {
                                                if (GridValue(ref RL_GRID0, i0 + 1, j0))
                                                {
                                                    if (GridValue(ref RL_GRID0, i0 + 1, j0 + 1))//向右向上
                                                    {
                                                        NxtID = 4;
                                                        InsertP.x++;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                        InsertP.y++;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    else//向右向右
                                                    {
                                                        NxtID = 2;
                                                        InsertP.x++;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    //NxtID = (GridValue(ref RL_GRID0, i0 + 1, j0 + 1)) ? 4 : 2; 
                                                }
                                                else//向右向下
                                                {
                                                    NxtID = 3;
                                                }
                                            }
                                            else //内边
                                            {
                                                if (GridValue(ref RL_GRID0, i0 + 1, j0 + 1))
                                                {
                                                    if (GridValue(ref RL_GRID0, i0 + 1, j0))//向右向下
                                                    {
                                                        NxtID = 3;
                                                        InsertP.x++;
                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                        InsertP.y--;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    else//向右向右
                                                    {
                                                        NxtID = 2;
                                                        InsertP.x++;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    //NxtID = (GridValue(ref RL_GRID0, i0 + 1, j0)) ? 3 : 2;
                                                }
                                                else NxtID = 4;//向右向上 
                                            }
                                            break;
                                        case 2:
                                            if (GridValue(ref RL_GRID0, i0 + 1, j0)) //内边
                                            {
                                                if (GridValue(ref RL_GRID0, i0, j0))
                                                {
                                                    if (GridValue(ref RL_GRID0, i0, j0 + 1))//向左向上
                                                    {
                                                        NxtID = 4;
                                                        InsertP.x--;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                        InsertP.y++;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    else //向左向左
                                                    {
                                                        NxtID = 1;
                                                        InsertP.x--;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    //NxtID = (GridValue(ref RL_GRID0, i0, j0 + 1)) ? 4 : 1;
                                                }
                                                else NxtID = 3; //向左向下
                                            }
                                            else //外边
                                            {
                                                if (GridValue(ref RL_GRID0, i0, j0 + 1))
                                                {
                                                    if (GridValue(ref RL_GRID0, i0, j0))//向左向下
                                                    {
                                                        NxtID = 3;
                                                        InsertP.x--;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                        InsertP.y--;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;

                                                    }
                                                    else//向左向左
                                                    {
                                                        NxtID = 1;
                                                        InsertP.x--;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    //NxtID = (GridValue(ref RL_GRID0, i0, j0)) ? 3 : 1;
                                                }
                                                else
                                                {
                                                    NxtID = 4;//向左向上
                                                }
                                            }
                                            break;
                                        case 3://向上
                                            if (GridValue(ref RL_GRID0, i0, j0))  //内边
                                            {
                                                if (GridValue(ref RL_GRID0, i0, j0 + 1))
                                                {
                                                    if (GridValue(ref RL_GRID0, i0 + 1, j0 + 1))//向上向右
                                                    {
                                                        NxtID = 2;
                                                        InsertP.y++;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                        InsertP.x++;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    else//向上向上
                                                    {
                                                        NxtID = 4;
                                                        InsertP.y++;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    //NxtID = (GridValue(ref RL_GRID0, i0 + 1, j0 + 1)) ? 2 : 4;
                                                }
                                                else NxtID = 1; //向上向左
                                            }
                                            else  //外边
                                            {
                                                if (GridValue(ref RL_GRID0, i0 + 1, j0 + 1))
                                                {
                                                    if (GridValue(ref RL_GRID0, i0, j0 + 1))
                                                    {
                                                        NxtID = 1;//从上往左
                                                        InsertP.y++;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                        InsertP.x--;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    else
                                                    {
                                                        NxtID = 4;//从上往上
                                                        InsertP.y++;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    //NxtID = (GridValue(ref RL_GRID0, i0, j0 + 1)) ? 1 : 4;
                                                }
                                                else
                                                {
                                                    NxtID = 2;//从上向右，插入点不变
                                                }
                                            }
                                            break;
                                        case 4://向下
                                            if (GridValue(ref RL_GRID0, i0, j0 + 1)) //外边
                                            {
                                                if (GridValue(ref RL_GRID0, i0, j0))
                                                {
                                                    if (GridValue(ref RL_GRID0, i0 + 1, j0))//向下向右
                                                    {
                                                        NxtID = 2;
                                                        InsertP.y--;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                        InsertP.x++;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    else//向下向下
                                                    {
                                                        NxtID = 3;
                                                        InsertP.y--;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    //NxtID = (GridValue(ref RL_GRID0, i0 + 1, j0)) ? 2 : 3;
                                                }
                                                else NxtID = 1; //向下向左
                                            }
                                            else
                                            {
                                                if (GridValue(ref RL_GRID0, i0 + 1, j0))
                                                {
                                                    if (GridValue(ref RL_GRID0, i0, j0))//向下向左
                                                    {
                                                        NxtID = 1;
                                                        InsertP.y--;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                        InsertP.x--;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    else//向下向下
                                                    {
                                                        NxtID = 3;
                                                        InsertP.y--;

                                                        if (InsertPointR(ref InsertP, ref RL_GRID0, R_GRID)) Changed = true;
                                                    }
                                                    //NxtID = (GridValue(ref RL_GRID0, i0, j0)) ? 1 : 3;
                                                }
                                                else NxtID = 2; //向下向右
                                            }
                                            break;
                                    }
                                    //2.2 根据下一点的走向获得下一点坐标

                                    switch (NxtID)
                                    {
                                        case 1:
                                            NxtP.x = CurP.x - 1; NxtP.y = CurP.y;

                                            break; //左
                                        case 2:
                                            NxtP.x = CurP.x + 1; NxtP.y = CurP.y;

                                            break; //右
                                        case 3:
                                            NxtP.x = CurP.x; NxtP.y = CurP.y - 1;             //下
                                            if (IsOnSECs(0, RL_GRID0[NxtP.y].rl_Num - 1, NxtP.x - 1, NxtP.y, ref RL_GRID0, ref Type, ref SecNo))
                                            {


                                                if (Type == 1)
                                                {
                                                    V1[NxtP.y][SecNo / 8] &= (Byte)~ConstVar.bits[SecNo % 8 + 1];

                                                }
                                                else
                                                {
                                                    V2[NxtP.y][SecNo / 8] &= (Byte)~ConstVar.bits[SecNo % 8 + 1];
                                                }


                                            }
                                            break;
                                        case 4:
                                            NxtP.x = CurP.x; NxtP.y = CurP.y + 1;             //上

                                            if (IsOnSECs(0, RL_GRID0[NxtP.y - 1].rl_Num - 1, NxtP.x - 1, NxtP.y - 1, ref RL_GRID0, ref Type, ref SecNo))
                                            {


                                                if (Type == 1)
                                                {
                                                    V1[NxtP.y - 1][SecNo / 8] &= (Byte)~ConstVar.bits[SecNo % 8 + 1];

                                                }
                                                else
                                                {
                                                    V2[NxtP.y - 1][SecNo / 8] &= (Byte)~ConstVar.bits[SecNo % 8 + 1];

                                                }



                                            }

                                            break;
                                    }
                                    //2.4.1 如果闭合则终止本弧追踪
                                    if (NxtP.x == P0.x && NxtP.y == P0.y)
                                    {
                                        break;
                                    }


                                    //2.5 循环重置——从下一点开始追踪下下点
                                    PreP = CurP;
                                    CurP = NxtP;

                                }
                            }
                    }
                    //所有行上的游程都遍历完，剥离右半边点
                    for (int i = 0; i < CELLs0.LINEs; i++)
                    {
                        int C1 = -1;
                        int C2 = -1;


                        for (int jjj = 0; jjj < R_GRID[i].rl_Num; jjj++)
                        {
                            if (jjj == 0)
                            {
                                C1 = (int)R_GRID[i].Line_Rs[jjj].C1; C2 = (int)R_GRID[i].Line_Rs[jjj].C1;
                                if (jjj == (R_GRID[i].rl_Num - 1))
                                {
                                    //C2 = (int)R_GRID[i].Line_Rs[jjj].C1;
                                    //擦除对应游程
                                    RemoveRL_Float(i, C1, C2, ref RL_GRID0);
                                    break;
                                }
                                continue;
                            }
                            else if ((C2 + 1) == (int)R_GRID[i].Line_Rs[jjj].C1)
                            {
                                C2++;
                                if (jjj == (R_GRID[i].rl_Num - 1))
                                {
                                    //C2 = (int)R_GRID[i].Line_Rs[jjj].C1;
                                    //擦除对应游程
                                    RemoveRL_Float(i, C1, C2, ref RL_GRID0);
                                    break;
                                }
                            }
                            else
                            {
                                //擦除对应游程
                                RemoveRL_Float(i, C1, C2, ref RL_GRID0);

                                C1 = (int)R_GRID[i].Line_Rs[jjj].C1;
                                C2 = (int)R_GRID[i].Line_Rs[jjj].C1;
                                if (jjj == (R_GRID[i].rl_Num - 1))
                                {
                                    //C2 = (int)R_GRID[i].Line_Rs[jjj].C1;
                                    //擦除对应游程
                                    RemoveRL_Float(i, C1, C2, ref RL_GRID0);
                                    break;
                                }
                            }


                        }

                    }
                    if (Changed == false)//无法继续剥离
                    {
                        break;
                    }
                    else
                    {
                        Changed = false;
                    }
                    loop++;






                    /*

                    //按照行扫描网格
                    for (int m = 1; m < CELLs0.LINEs - 1; m++)
                    {
                        //按行遍历游程
                        for(int n=0;n< RL_GRID0[m].rl_Num;n++)
                        {

                            int C1 = -1;
                            int C2 = -1;
                            for (int k= Convert.ToInt32(RL_GRID0[m].Line_RLs[n].C1); k<=Convert.ToInt32( RL_GRID0[m].Line_RLs[n].C2);k++)
                            {
                                int p2 = GetValue_GRID(m + 1, k,ref RL_GRID0);
                                int p3 = GetValue_GRID(m + 1, k+1, ref RL_GRID0);
                                int p4 = GetValue_GRID(m, k + 1, ref RL_GRID0);
                                int p5 = GetValue_GRID(m-1, k + 1, ref RL_GRID0);
                                int p6 = GetValue_GRID(m-1, k, ref RL_GRID0);
                                int p7 = GetValue_GRID(m - 1, k-1, ref RL_GRID0);
                                int p8 = GetValue_GRID(m, k-1, ref RL_GRID0);
                                int p9 = GetValue_GRID(m + 1, k-1, ref RL_GRID0);
                                if ((p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9) >= 2 && (p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9) <= 6)
                                {
                                    int ap = 0;
                                    if (p2 == 0 && p3 == 1) ++ap;
                                    if (p3 == 0 && p4 == 1) ++ap;
                                    if (p4 == 0 && p5 == 1) ++ap;
                                    if (p5 == 0 && p6 == 1) ++ap;
                                    if (p6 == 0 && p7 == 1) ++ap;
                                    if (p7 == 0 && p8 == 1) ++ap;
                                    if (p8 == 0 && p9 == 1) ++ap;
                                    if (p9 == 0 && p2 == 1) ++ap;
                                    if (ap == 1)
                                    {
                                        if (p2 * p4 * p6 == 0)
                                        {
                                            if (p4 * p6 * p8 == 0)
                                            {
                                                //标记 
                                                if (C1 == -1) { C1 = k; C2 = k; }  //左侧节点
                                                else C2 = k;
                                                //RemoveLabel[m, n] = true;
                                            }
                                            else
                                            {
                                                if (C1 != -1 && C2 != -1)//将新游程插入
                                                {
                                                    InsertRL_Float(m, C1, C2, ref RL_GRID1);
                                                    C1 = -1;
                                                    C2 = -1;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (C1 != -1 && C2 != -1)//将新游程插入
                                            {
                                                InsertRL_Float(m, C1, C2, ref RL_GRID1);
                                                C1 = -1;
                                                C2 = -1;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (C1 != -1 && C2 != -1)//将新游程插入
                                        {
                                            InsertRL_Float(m, C1, C2, ref RL_GRID1);
                                            C1 = -1;
                                            C2 = -1;
                                        }
                                    }
                                }
                                else
                                {
                                    if(C1 != -1&& C2 != -1)//将新游程插入
                                    {
                                        InsertRL_Float(m, C1, C2, ref RL_GRID1);
                                        C1 = -1;
                                        C2 = -1;
                                    }
                                }

                                
                            }
                            if (C1 != -1 && C2 != -1)//将新游程插入
                            {
                                InsertRL_Float(m, C1, C2, ref RL_GRID1);
                                C1 = -1;
                                C2 = -1;
                            }


                        }
                    }

                    for(int i=0; i<CELLs0.LINEs; i++)
                    {
                        if(RL_GRID1[i].rl_Num>0)
                        {
                            for(int j=0;j< RL_GRID1[i].rl_Num;j++)
                            {
                                RemoveRL_Float(i, RL_GRID1[i].Line_RLs[j].C1, RL_GRID1[i].Line_RLs[j].C2, ref RL_GRID0);
                                Changed = true;
                            }
                        }
                    }

                    


                    if (Changed == false)//无法继续剥离
                    {
                        break;
                    }
                    else
                    {
                        Changed = false;
                    }
                    */

                    /*
                    FLOAT_RL_LINE[] RL_GRID2 = new FLOAT_RL_LINE[CELLs0.LINEs];
                    for (int i = 0; i < CELLs0.LINEs; i++)
                    {
                        RL_GRID2[i] = new FLOAT_RL_LINE();
                        RL_GRID2[i].rl_Num = 0;
                        RL_GRID2[i].Line_RLs = new List<FLOAT_RL_LinkTable>();
                        
                    }

                    //按照行扫描网格
                    for (int m = 1; m < CELLs0.LINEs - 1; m++)
                    {
                        //按行遍历游程
                        for (int n = 0; n < RL_GRID0[m].rl_Num; n++)
                        {
                            int C1 = -1;
                            int C2 = -1;

                            for (int k = Convert.ToInt32(RL_GRID0[m].Line_RLs[n].C1); k <= Convert.ToInt32(RL_GRID0[m].Line_RLs[n].C2); k++)
                            {
                                // p9 p2 p3
                                // p8 p1 p4
                                // p7 p6 p5
                                

                                int p2 = GetValue_GRID(m + 1, k, ref RL_GRID0);
                                int p3 = GetValue_GRID(m + 1, k + 1, ref RL_GRID0);
                                int p4 = GetValue_GRID(m, k + 1, ref RL_GRID0);
                                int p5 = GetValue_GRID(m - 1, k + 1, ref RL_GRID0);
                                int p6 = GetValue_GRID(m - 1, k, ref RL_GRID0);
                                int p7 = GetValue_GRID(m - 1, k - 1, ref RL_GRID0);
                                int p8 = GetValue_GRID(m, k - 1, ref RL_GRID0);
                                int p9 = GetValue_GRID(m + 1, k - 1, ref RL_GRID0);

                                

                                if ((p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9) >= 2 && (p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9) <= 6)
                                {
                                    int ap = 0;
                                    if (p2 == 0 && p3 == 1) ++ap;
                                    if (p3 == 0 && p4 == 1) ++ap;
                                    if (p4 == 0 && p5 == 1) ++ap;
                                    if (p5 == 0 && p6 == 1) ++ap;
                                    if (p6 == 0 && p7 == 1) ++ap;
                                    if (p7 == 0 && p8 == 1) ++ap;
                                    if (p8 == 0 && p9 == 1) ++ap;
                                    if (p9 == 0 && p2 == 1) ++ap;
                                    if (ap == 1)
                                    {
                                        if (p2 * p4 * p8 == 0)
                                        {
                                            if (p2 * p6 * p8 == 0)
                                            {
                                                //标记  
                                                //标记 
                                                //标记 
                                                if (C1 == -1) { C1 = k; C2 = k; }  //左侧节点
                                                else C2 = k;
                                                //RemoveLabel[m, n] = true;
                                            }
                                            else
                                            {
                                                if (C1 != -1 && C2 != -1)//将新游程插入
                                                {
                                                    InsertRL_Float(m, C1, C2, ref RL_GRID2);
                                                    C1 = -1;
                                                    C2 = -1;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (C1 != -1 && C2 != -1)//将新游程插入
                                            {
                                                InsertRL_Float(m, C1, C2, ref RL_GRID2);
                                                C1 = -1;
                                                C2 = -1;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (C1 != -1 && C2 != -1)//将新游程插入
                                        {
                                            InsertRL_Float(m, C1, C2, ref RL_GRID2);
                                            C1 = -1;
                                            C2 = -1;
                                        }
                                    }
                                }
                                else
                                {
                                    if (C1 != -1 && C2 != -1)//将新游程插入
                                    {
                                        InsertRL_Float(m, C1, C2, ref RL_GRID2);
                                        C1 = -1;
                                        C2 = -1;
                                    }
                                }
                            }
                            if (C1 != -1 && C2 != -1)//将新游程插入
                            {
                                InsertRL_Float(m, C1, C2, ref RL_GRID2);
                                C1 = -1;
                                C2 = -1;
                            }

                        }
                    }

                    for (int i = 1; i < CELLs0.LINEs - 1; i++)
                    {
                        if (RL_GRID2[i].rl_Num > 0)
                        {
                            for (int j = 0; j < RL_GRID2[i].rl_Num; j++)
                            {
                                RemoveRL_Float(i, RL_GRID2[i].Line_RLs[j].C1, RL_GRID2[i].Line_RLs[j].C2, ref RL_GRID0);
                                Changed = true;
                            }
                        }
                    }

                    if (Changed == false)//无法继续剥离
                    {
                        break;
                    }
                    else
                    {
                        Changed = false;
                    }


                    loop++;
                */


                }
                //清除游程缝隙
                //Clear_TinyGap(ref RL_GRID0, ref CELLs0);

                //输出所有栅格点
                /*
                if(ii==46)
                {
                    List<IPoint> pntCol = new List<IPoint>();


                    for (int i = 1; i < CELLs0.LINEs - 1; i++)
                    {
                        if (RL_GRID0[i].rl_Num > 0)
                        {
                            for (int j = 0; j < RL_GRID0[i].rl_Num; j++)
                            {
                                for(int k = (int)RL_GRID0[i].Line_RLs[j].C1; k<= (int)RL_GRID0[i].Line_RLs[j].C2;k++)
                                {
                                    IPoint pnt = new Point();
                                    pnt.X = CELLs0.xmin + CELLs0.dxy * (k + 0.5);
                                    pnt.Y = CELLs0.ymin + CELLs0.dxy * (i + 0.5);
                                    pntCol.Add(pnt);
                                }
                                
                            }
                        }

                        

                    }
                    Feature2Shapefile(CenterLineDir, "zxx.shp", ref pntCol);
                    return;
                }
                */

                //输出线段

                //第一步，从端点开始，找到所有的骨架线
                for (; ; )
                {
                    bool changed = false;

                    for (int i = 1; i < CELLs0.LINEs - 1; i++)
                    {
                        if (RL_GRID0[i].rl_Num > 0)
                        {
                            for (int j = 0; j < RL_GRID0[i].rl_Num; j++)
                            {



                                int k = (int)RL_GRID0[i].Line_RLs[j].C1;
                                //这个点必须是端点
                                // p9 p2 p3
                                // p8 p1 p4
                                // p7 p6 p5
                                int p2 = GetValue_GRID(i + 1, k, ref RL_GRID0);
                                int p3 = GetValue_GRID(i + 1, k + 1, ref RL_GRID0);
                                int p4 = GetValue_GRID(i, k + 1, ref RL_GRID0);
                                int p5 = GetValue_GRID(i - 1, k + 1, ref RL_GRID0);
                                int p6 = GetValue_GRID(i - 1, k, ref RL_GRID0);
                                int p7 = GetValue_GRID(i - 1, k - 1, ref RL_GRID0);
                                int p8 = GetValue_GRID(i, k - 1, ref RL_GRID0);
                                int p9 = GetValue_GRID(i + 1, k - 1, ref RL_GRID0);
                                //if ((p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9) != 1 && (p2 +  p4 +  p6 + p8) < 2) continue;
                                if ((p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9) == 1) //左侧是端点或者是连接点
                                {

                                    changed = true;

                                    int tracei = -1;
                                    int tracek = -1;

                                    ARC pArc = new ARC();
                                    pArc.pNum = 0;
                                    pArc.pts = new PNT[ConstVar._MAX_ARCNUM_aMap];
                                    bool[] SelectID = new bool[ConstVar._MAX_ARCNUM_aMap];
                                    for (int id = 0; id < ConstVar._MAX_ARCNUM_aMap; id++)
                                    {
                                        SelectID[id] = false;
                                    }




                                    tracei = i;
                                    tracek = k;

                                    pArc.pts[pArc.pNum].x = CELLs0.xmin + CELLs0.dxy * (tracek + 0.5);
                                    pArc.pts[pArc.pNum].y = CELLs0.ymin + CELLs0.dxy * (tracei + 0.5);
                                    SelectID[pArc.pNum] = true;
                                    pArc.pNum++;

                                    if ((p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9) == 1)
                                    {
                                        //判断游程上栅格数量
                                        //游程只有一个网格
                                        if (RL_GRID0[i].Line_RLs[j].C1 == RL_GRID0[i].Line_RLs[j].C2)
                                        {
                                            RL_GRID0[i].Line_RLs.RemoveAt(j);
                                            RL_GRID0[i].rl_Num--;
                                            j--;
                                        }
                                        //如果游程上不止一个网格
                                        else if (RL_GRID0[i].Line_RLs[j].C1 < RL_GRID0[i].Line_RLs[j].C2)
                                        {
                                            RL_GRID0[i].Line_RLs[j].C1++;

                                        }
                                    }




                                    for (; ; )
                                    {

                                        int p;
                                        int nextO = -1;

                                        // p9 p2 p3
                                        // p8 p1 p4
                                        // p7 p6 p5
                                        //int p2 = GetValue_GRID(i + 1, k, ref RL_GRID0);
                                        //int p3 = GetValue_GRID(i + 1, k + 1, ref RL_GRID0);
                                        //int p4 = GetValue_GRID(i, k + 1, ref RL_GRID0);
                                        //int p5 = GetValue_GRID(i - 1, k + 1, ref RL_GRID0);
                                        //int p6 = GetValue_GRID(i - 1, k, ref RL_GRID0);
                                        //int p7 = GetValue_GRID(i - 1, k - 1, ref RL_GRID0);
                                        //int p8 = GetValue_GRID(i, k - 1, ref RL_GRID0);
                                        //int p9 = GetValue_GRID(i + 1, k - 1, ref RL_GRID0);

                                        //---------------------------------查找上一游程
                                        p = -1;
                                        if (GetValue_GRID(tracei + 1, tracek, ref RL_GRID0, ref p) == 1)
                                        {
                                            nextO = 0;
                                            tracei = tracei + 1;
                                            //tracek = tracek;  
                                        }
                                        //---------------------------------查找右侧游程
                                        else if (GetValue_GRID(tracei, tracek + 1, ref RL_GRID0, ref p) == 1)
                                        {
                                            nextO = 2;
                                            //tracei = tracei;
                                            tracek = tracek + 1;
                                        }
                                        //---------------------------------查找下侧游程
                                        else if (GetValue_GRID(tracei - 1, tracek, ref RL_GRID0, ref p) == 1)
                                        {
                                            nextO = 4;
                                            tracei = tracei - 1;
                                            //tracek = tracek;
                                        }
                                        //---------------------------------查找左侧游程
                                        else if (GetValue_GRID(tracei, tracek - 1, ref RL_GRID0, ref p) == 1)
                                        {
                                            nextO = 6;
                                            //tracei = tracei;
                                            tracek = tracek - 1;
                                        }

                                        //右上
                                        else if (GetValue_GRID(tracei + 1, tracek + 1, ref RL_GRID0, ref p) == 1)
                                        {
                                            nextO = 1;
                                            tracei = tracei + 1;
                                            tracek = tracek + 1;
                                        }
                                        //右下
                                        else if (GetValue_GRID(tracei - 1, tracek + 1, ref RL_GRID0, ref p) == 1)
                                        {
                                            nextO = 3;
                                            tracei = tracei - 1;
                                            tracek = tracek + 1;
                                        }
                                        //左下
                                        else if (GetValue_GRID(tracei - 1, tracek - 1, ref RL_GRID0, ref p) == 1)
                                        {
                                            nextO = 5;
                                            tracei = tracei - 1;
                                            tracek = tracek - 1;
                                        }
                                        //左上
                                        else if (GetValue_GRID(tracei + 1, tracek - 1, ref RL_GRID0, ref p) == 1)
                                        {
                                            nextO = 7;
                                            tracei = tracei + 1;
                                            tracek = tracek - 1;
                                        }

                                        else
                                        {

                                            break;
                                        }


                                        pArc.pts[pArc.pNum].x = CELLs0.xmin + CELLs0.dxy * (tracek + 0.5);
                                        pArc.pts[pArc.pNum].y = CELLs0.ymin + CELLs0.dxy * (tracei + 0.5);
                                        SelectID[pArc.pNum] = true;
                                        pArc.pNum++;




                                        ////////////////增加的代码

                                        //需要判断这个新的点是不是分叉点，如果是就要在这里终止
                                        //
                                        // p9 p2 p3
                                        // p8 p1 p4
                                        // p7 p6 p5

                                        int pp2 = GetValue_GRID(tracei + 1, tracek, ref RL_GRID0);
                                        int pp3 = GetValue_GRID(tracei + 1, tracek + 1, ref RL_GRID0);
                                        int pp4 = GetValue_GRID(tracei, tracek + 1, ref RL_GRID0);
                                        int pp5 = GetValue_GRID(tracei - 1, tracek + 1, ref RL_GRID0);
                                        int pp6 = GetValue_GRID(tracei - 1, tracek, ref RL_GRID0);
                                        int pp7 = GetValue_GRID(tracei - 1, tracek - 1, ref RL_GRID0);
                                        int pp8 = GetValue_GRID(tracei, tracek - 1, ref RL_GRID0);
                                        int pp9 = GetValue_GRID(tracei + 1, tracek - 1, ref RL_GRID0);

                                        if ((pp2 + pp4 + pp6 + pp8) >= 2)
                                        {

                                            break;
                                        }

                                        else if ((pp2 + pp3 + pp4 + pp5 + pp6 + pp7 + pp8 + pp9) == 2)//可能是分叉点
                                        {
                                            if ((pp2 + pp3) != 2 && (pp3 + pp4) != 2 && (pp4 + pp5) != 2 && (pp5 + pp6) != 2 && (pp6 + pp7) != 2 && (pp7 + pp8) != 2 && (pp8 + pp9) != 2 && (pp9 + pp2) != 2) { break; }

                                        }

                                        ////////////////增加的代码

                                        //分两种情况
                                        //游程只有一个网格
                                        if (RL_GRID0[tracei].Line_RLs[p].C1 == RL_GRID0[tracei].Line_RLs[p].C2)
                                        {
                                            RL_GRID0[tracei].Line_RLs.RemoveAt(p);
                                            RL_GRID0[tracei].rl_Num--;
                                        }
                                        //如果游程上不止一个网格
                                        else if (RL_GRID0[tracei].Line_RLs[p].C1 < RL_GRID0[tracei].Line_RLs[p].C2)
                                        {
                                            if (RL_GRID0[tracei].Line_RLs[p].C1 == tracek)
                                            {
                                                RL_GRID0[tracei].Line_RLs[p].C1++;
                                            }
                                            else if (RL_GRID0[tracei].Line_RLs[p].C2 == tracek)
                                            {
                                                RL_GRID0[tracei].Line_RLs[p].C2--;
                                            }
                                            //找到的点  处于中间位置
                                            //else break;
                                        }





                                    }


                                    //存储线
                                    //存储线段
                                    ARC_ARJ(ref pArc, ref CentetLines[ii].arcs[CentetLines[ii].aNum++], -1, ref SelectID);



                                    break;
                                }

                                k = (int)RL_GRID0[i].Line_RLs[j].C2;
                                //这个点必须是端点
                                // p9 p2 p3
                                // p8 p1 p4
                                // p7 p6 p5
                                p2 = GetValue_GRID(i + 1, k, ref RL_GRID0);
                                p3 = GetValue_GRID(i + 1, k + 1, ref RL_GRID0);
                                p4 = GetValue_GRID(i, k + 1, ref RL_GRID0);
                                p5 = GetValue_GRID(i - 1, k + 1, ref RL_GRID0);
                                p6 = GetValue_GRID(i - 1, k, ref RL_GRID0);
                                p7 = GetValue_GRID(i - 1, k - 1, ref RL_GRID0);
                                p8 = GetValue_GRID(i, k - 1, ref RL_GRID0);
                                p9 = GetValue_GRID(i + 1, k - 1, ref RL_GRID0);

                                if ((p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9) == 1) //右侧是端点
                                {

                                    changed = true;

                                    int tracei = -1;
                                    int tracek = -1;

                                    ARC pArc = new ARC();
                                    pArc.pNum = 0;
                                    pArc.pts = new PNT[ConstVar._MAX_ARCNUM_aMap];
                                    bool[] SelectID = new bool[ConstVar._MAX_ARCNUM_aMap];
                                    for (int id = 0; id < ConstVar._MAX_ARCNUM_aMap; id++)
                                    {
                                        SelectID[id] = false;
                                    }




                                    tracei = i;
                                    tracek = k;

                                    pArc.pts[pArc.pNum].x = CELLs0.xmin + CELLs0.dxy * (tracek + 0.5);
                                    pArc.pts[pArc.pNum].y = CELLs0.ymin + CELLs0.dxy * (tracei + 0.5);
                                    SelectID[pArc.pNum] = true;
                                    pArc.pNum++;

                                    if ((p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9) == 1)
                                    {
                                        //判断游程上栅格数量
                                        //游程只有一个网格
                                        if (RL_GRID0[i].Line_RLs[j].C1 == RL_GRID0[i].Line_RLs[j].C2)
                                        {
                                            RL_GRID0[i].Line_RLs.RemoveAt(j);
                                            RL_GRID0[i].rl_Num--;
                                            j--;
                                        }
                                        //如果游程上不止一个网格
                                        else if (RL_GRID0[i].Line_RLs[j].C1 < RL_GRID0[i].Line_RLs[j].C2)
                                        {
                                            RL_GRID0[i].Line_RLs[j].C2--;

                                        }
                                    }




                                    for (; ; )
                                    {

                                        int p;
                                        int nextO = -1;

                                        // p9 p2 p3
                                        // p8 p1 p4
                                        // p7 p6 p5
                                        //int p2 = GetValue_GRID(i + 1, k, ref RL_GRID0);
                                        //int p3 = GetValue_GRID(i + 1, k + 1, ref RL_GRID0);
                                        //int p4 = GetValue_GRID(i, k + 1, ref RL_GRID0);
                                        //int p5 = GetValue_GRID(i - 1, k + 1, ref RL_GRID0);
                                        //int p6 = GetValue_GRID(i - 1, k, ref RL_GRID0);
                                        //int p7 = GetValue_GRID(i - 1, k - 1, ref RL_GRID0);
                                        //int p8 = GetValue_GRID(i, k - 1, ref RL_GRID0);
                                        //int p9 = GetValue_GRID(i + 1, k - 1, ref RL_GRID0);

                                        //---------------------------------查找上一游程
                                        p = -1;
                                        if (GetValue_GRID(tracei + 1, tracek, ref RL_GRID0, ref p) == 1)
                                        {
                                            nextO = 0;
                                            tracei = tracei + 1;
                                            //tracek = tracek;  
                                        }
                                        //---------------------------------查找右侧游程
                                        else if (GetValue_GRID(tracei, tracek + 1, ref RL_GRID0, ref p) == 1)
                                        {
                                            nextO = 2;
                                            //tracei = tracei;
                                            tracek = tracek + 1;
                                        }
                                        //---------------------------------查找下侧游程
                                        else if (GetValue_GRID(tracei - 1, tracek, ref RL_GRID0, ref p) == 1)
                                        {
                                            nextO = 4;
                                            tracei = tracei - 1;
                                            //tracek = tracek;
                                        }
                                        //---------------------------------查找左侧游程
                                        else if (GetValue_GRID(tracei, tracek - 1, ref RL_GRID0, ref p) == 1)
                                        {
                                            nextO = 6;
                                            //tracei = tracei;
                                            tracek = tracek - 1;
                                        }

                                        //右上
                                        else if (GetValue_GRID(tracei + 1, tracek + 1, ref RL_GRID0, ref p) == 1)
                                        {
                                            nextO = 1;
                                            tracei = tracei + 1;
                                            tracek = tracek + 1;
                                        }
                                        //右下
                                        else if (GetValue_GRID(tracei - 1, tracek + 1, ref RL_GRID0, ref p) == 1)
                                        {
                                            nextO = 3;
                                            tracei = tracei - 1;
                                            tracek = tracek + 1;
                                        }
                                        //左下
                                        else if (GetValue_GRID(tracei - 1, tracek - 1, ref RL_GRID0, ref p) == 1)
                                        {
                                            nextO = 5;
                                            tracei = tracei - 1;
                                            tracek = tracek - 1;
                                        }
                                        //左上
                                        else if (GetValue_GRID(tracei + 1, tracek - 1, ref RL_GRID0, ref p) == 1)
                                        {
                                            nextO = 7;
                                            tracei = tracei + 1;
                                            tracek = tracek - 1;
                                        }

                                        else
                                        {

                                            break;
                                        }


                                        pArc.pts[pArc.pNum].x = CELLs0.xmin + CELLs0.dxy * (tracek + 0.5);
                                        pArc.pts[pArc.pNum].y = CELLs0.ymin + CELLs0.dxy * (tracei + 0.5);
                                        SelectID[pArc.pNum] = true;
                                        pArc.pNum++;



                                        ////////////////增加的代码

                                        //需要判断这个新的点是不是分叉点，如果是就要在这里终止
                                        //
                                        // p9 p2 p3
                                        // p8 p1 p4
                                        // p7 p6 p5

                                        int pp2 = GetValue_GRID(tracei + 1, tracek, ref RL_GRID0);
                                        int pp3 = GetValue_GRID(tracei + 1, tracek + 1, ref RL_GRID0);
                                        int pp4 = GetValue_GRID(tracei, tracek + 1, ref RL_GRID0);
                                        int pp5 = GetValue_GRID(tracei - 1, tracek + 1, ref RL_GRID0);
                                        int pp6 = GetValue_GRID(tracei - 1, tracek, ref RL_GRID0);
                                        int pp7 = GetValue_GRID(tracei - 1, tracek - 1, ref RL_GRID0);
                                        int pp8 = GetValue_GRID(tracei, tracek - 1, ref RL_GRID0);
                                        int pp9 = GetValue_GRID(tracei + 1, tracek - 1, ref RL_GRID0);

                                        bool flag = false;
                                        if ((pp2 + pp4 + pp6 + pp8) >= 2)
                                        {

                                            break;
                                        }

                                        else if ((pp2 + pp3 + pp4 + pp5 + pp6 + pp7 + pp8 + pp9) == 2)//可能是分叉点
                                        {
                                            if ((pp2 + pp3) != 2 && (pp3 + pp4) != 2 && (pp4 + pp5) != 2 && (pp5 + pp6) != 2 && (pp6 + pp7) != 2 && (pp7 + pp8) != 2 && (pp8 + pp9) != 2 && (pp9 + pp2) != 2) { break; }
                                        }


                                        ////////////////增加的代码

                                        //分两种情况
                                        //游程只有一个网格
                                        if (RL_GRID0[tracei].Line_RLs[p].C1 == RL_GRID0[tracei].Line_RLs[p].C2)
                                        {
                                            RL_GRID0[tracei].Line_RLs.RemoveAt(p);
                                            RL_GRID0[tracei].rl_Num--;
                                        }
                                        //如果游程上不止一个网格
                                        else if (RL_GRID0[tracei].Line_RLs[p].C1 < RL_GRID0[tracei].Line_RLs[p].C2)
                                        {
                                            if (RL_GRID0[tracei].Line_RLs[p].C1 == tracek)
                                            {
                                                RL_GRID0[tracei].Line_RLs[p].C1++;
                                            }
                                            else if (RL_GRID0[tracei].Line_RLs[p].C2 == tracek)
                                            {
                                                RL_GRID0[tracei].Line_RLs[p].C2--;
                                            }
                                            //找到的点  处于中间位置
                                            //else break;
                                        }





                                    }


                                    //存储线
                                    //存储线段
                                    ARC_ARJ(ref pArc, ref CentetLines[ii].arcs[CentetLines[ii].aNum++], -1, ref SelectID);



                                    break;
                                }
                            }




                        }



                    }
                    if (changed == false)
                    {
                        break;
                    }

                }

                //第二步，从连接点开始，找到剩余的骨架线
                for (; ; )
                {
                    bool changed = false;

                    for (int i = 1; i < CELLs0.LINEs - 1; i++)
                    {
                        if (RL_GRID0[i].rl_Num > 0)
                        {
                            for (int j = 0; j < RL_GRID0[i].rl_Num; j++)
                            {

                                //if (RL_GRID0[i].Line_RLs[j].C1 != RL_GRID0[i].Line_RLs[j].C2) continue;

                                int k = (int)RL_GRID0[i].Line_RLs[j].C1;
                                //这个点必须是端点
                                // p9 p2 p3
                                // p8 p1 p4
                                // p7 p6 p5
                                int p2 = GetValue_GRID(i + 1, k, ref RL_GRID0);
                                int p3 = GetValue_GRID(i + 1, k + 1, ref RL_GRID0);
                                int p4 = GetValue_GRID(i, k + 1, ref RL_GRID0);
                                int p5 = GetValue_GRID(i - 1, k + 1, ref RL_GRID0);
                                int p6 = GetValue_GRID(i - 1, k, ref RL_GRID0);
                                int p7 = GetValue_GRID(i - 1, k - 1, ref RL_GRID0);
                                int p8 = GetValue_GRID(i, k - 1, ref RL_GRID0);
                                int p9 = GetValue_GRID(i + 1, k - 1, ref RL_GRID0);



                                changed = true;

                                int tracei = -1;
                                int tracek = -1;

                                ARC pArc = new ARC();
                                pArc.pNum = 0;
                                pArc.pts = new PNT[ConstVar._MAX_ARCNUM_aMap];
                                bool[] SelectID = new bool[ConstVar._MAX_ARCNUM_aMap];
                                for (int id = 0; id < ConstVar._MAX_ARCNUM_aMap; id++)
                                {
                                    SelectID[id] = false;
                                }




                                tracei = i;
                                tracek = k;

                                pArc.pts[pArc.pNum].x = CELLs0.xmin + CELLs0.dxy * (tracek + 0.5);
                                pArc.pts[pArc.pNum].y = CELLs0.ymin + CELLs0.dxy * (tracei + 0.5);
                                SelectID[pArc.pNum] = true;
                                pArc.pNum++;

                                RL_GRID0[i].Line_RLs.RemoveAt(j);
                                RL_GRID0[i].rl_Num--;
                                j--;


                                for (; ; )
                                {

                                    int p;
                                    int nextO = -1;

                                    // p9 p2 p3
                                    // p8 p1 p4
                                    // p7 p6 p5
                                    //int p2 = GetValue_GRID(i + 1, k, ref RL_GRID0);
                                    //int p3 = GetValue_GRID(i + 1, k + 1, ref RL_GRID0);
                                    //int p4 = GetValue_GRID(i, k + 1, ref RL_GRID0);
                                    //int p5 = GetValue_GRID(i - 1, k + 1, ref RL_GRID0);
                                    //int p6 = GetValue_GRID(i - 1, k, ref RL_GRID0);
                                    //int p7 = GetValue_GRID(i - 1, k - 1, ref RL_GRID0);
                                    //int p8 = GetValue_GRID(i, k - 1, ref RL_GRID0);
                                    //int p9 = GetValue_GRID(i + 1, k - 1, ref RL_GRID0);

                                    //---------------------------------查找上一游程
                                    p = -1;
                                    if (GetValue_GRID(tracei + 1, tracek, ref RL_GRID0, ref p) == 1)
                                    {
                                        nextO = 0;
                                        tracei = tracei + 1;
                                        //tracek = tracek;  
                                    }
                                    //---------------------------------查找右侧游程
                                    else if (GetValue_GRID(tracei, tracek + 1, ref RL_GRID0, ref p) == 1)
                                    {
                                        nextO = 2;
                                        //tracei = tracei;
                                        tracek = tracek + 1;
                                    }
                                    //---------------------------------查找下侧游程
                                    else if (GetValue_GRID(tracei - 1, tracek, ref RL_GRID0, ref p) == 1)
                                    {
                                        nextO = 4;
                                        tracei = tracei - 1;
                                        //tracek = tracek;
                                    }
                                    //---------------------------------查找左侧游程
                                    else if (GetValue_GRID(tracei, tracek - 1, ref RL_GRID0, ref p) == 1)
                                    {
                                        nextO = 6;
                                        //tracei = tracei;
                                        tracek = tracek - 1;
                                    }

                                    //右上
                                    else if (GetValue_GRID(tracei + 1, tracek + 1, ref RL_GRID0, ref p) == 1)
                                    {
                                        nextO = 1;
                                        tracei = tracei + 1;
                                        tracek = tracek + 1;
                                    }
                                    //右下
                                    else if (GetValue_GRID(tracei - 1, tracek + 1, ref RL_GRID0, ref p) == 1)
                                    {
                                        nextO = 3;
                                        tracei = tracei - 1;
                                        tracek = tracek + 1;
                                    }
                                    //左下
                                    else if (GetValue_GRID(tracei - 1, tracek - 1, ref RL_GRID0, ref p) == 1)
                                    {
                                        nextO = 5;
                                        tracei = tracei - 1;
                                        tracek = tracek - 1;
                                    }
                                    //左上
                                    else if (GetValue_GRID(tracei + 1, tracek - 1, ref RL_GRID0, ref p) == 1)
                                    {
                                        nextO = 7;
                                        tracei = tracei + 1;
                                        tracek = tracek - 1;
                                    }

                                    else
                                    {

                                        break;
                                    }


                                    pArc.pts[pArc.pNum].x = CELLs0.xmin + CELLs0.dxy * (tracek + 0.5);
                                    pArc.pts[pArc.pNum].y = CELLs0.ymin + CELLs0.dxy * (tracei + 0.5);
                                    SelectID[pArc.pNum] = true;
                                    pArc.pNum++;




                                    ////////////////增加的代码

                                    //需要判断这个新的点是不是分叉点，如果是就要在这里终止
                                    //
                                    // p9 p2 p3
                                    // p8 p1 p4
                                    // p7 p6 p5

                                    int pp2 = GetValue_GRID(tracei + 1, tracek, ref RL_GRID0);
                                    int pp3 = GetValue_GRID(tracei + 1, tracek + 1, ref RL_GRID0);
                                    int pp4 = GetValue_GRID(tracei, tracek + 1, ref RL_GRID0);
                                    int pp5 = GetValue_GRID(tracei - 1, tracek + 1, ref RL_GRID0);
                                    int pp6 = GetValue_GRID(tracei - 1, tracek, ref RL_GRID0);
                                    int pp7 = GetValue_GRID(tracei - 1, tracek - 1, ref RL_GRID0);
                                    int pp8 = GetValue_GRID(tracei, tracek - 1, ref RL_GRID0);
                                    int pp9 = GetValue_GRID(tracei + 1, tracek - 1, ref RL_GRID0);

                                    if ((pp2 + pp4 + pp6 + pp8) >= 2)
                                    {

                                        break;
                                    }

                                    else if ((pp2 + pp3 + pp4 + pp5 + pp6 + pp7 + pp8 + pp9) == 2)//可能是分叉点
                                    {
                                        if ((pp2 + pp3) != 2 && (pp3 + pp4) != 2 && (pp4 + pp5) != 2 && (pp5 + pp6) != 2 && (pp6 + pp7) != 2 && (pp7 + pp8) != 2 && (pp8 + pp9) != 2 && (pp9 + pp2) != 2) { break; }

                                    }

                                    ////////////////增加的代码

                                    //分两种情况
                                    //游程只有一个网格
                                    if (RL_GRID0[tracei].Line_RLs[p].C1 == RL_GRID0[tracei].Line_RLs[p].C2)
                                    {
                                        RL_GRID0[tracei].Line_RLs.RemoveAt(p);
                                        RL_GRID0[tracei].rl_Num--;
                                    }
                                    //如果游程上不止一个网格
                                    else if (RL_GRID0[tracei].Line_RLs[p].C1 < RL_GRID0[tracei].Line_RLs[p].C2)
                                    {
                                        if (RL_GRID0[tracei].Line_RLs[p].C1 == tracek)
                                        {
                                            RL_GRID0[tracei].Line_RLs[p].C1++;
                                        }
                                        else if (RL_GRID0[tracei].Line_RLs[p].C2 == tracek)
                                        {
                                            RL_GRID0[tracei].Line_RLs[p].C2--;
                                        }
                                        //找到的点  处于中间位置
                                        //else break;
                                    }





                                }


                                //存储线
                                //存储线段
                                ARC_ARJ(ref pArc, ref CentetLines[ii].arcs[CentetLines[ii].aNum++], -1, ref SelectID);



                                break;



                            }




                        }



                    }
                    if (changed == false)
                    {
                        break;
                    }

                }
            }

            
            

            //输出测试
            //Feature2Shapefile(CenterLineDir, "zxx.shp", ref CentetLines, riverpset.PatchNum);

            //将线输出到txt中
            int polylinenum = 0;
            for(int i=0;i< riverpset.PatchNum;i++)
            {
                polylinenum += CentetLines[i].aNum;
            }

            //导出到txt中
            FileStream fs1 = new FileStream(linedie + "/polyline.txt", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs1);
            sw.WriteLine("POLYLINE," + polylinenum.ToString());

            
            for (int i = 0; i < riverpset.PatchNum; i++)
            {
                for(int j=0;j< CentetLines[i].aNum;j++)
                {
                    sw.WriteLine("LINE," +  CentetLines[i].arcs[j].pNum.ToString());
                    for (int k=0;k< CentetLines[i].arcs[j].pNum;k++)
                    {
                        sw.WriteLine(CentetLines[i].arcs[j].pts[k].x.ToString() +","+ CentetLines[i].arcs[j].pts[k].y.ToString());
                    }
                }
            }
            sw.WriteLine("END");
            sw.Close();
        }
    

        public void CenterLineCompt(string CenterLineDir, ref string Message)
        {
            StreamReader sr = new StreamReader(CenterLineDir);
            //////////////读水域面文件////////////////////

            MAPARC riverbuffer = new MAPARC();
            PATCHSET riverpset = new PATCHSET();
            //分析多边形数目
            string readline = sr.ReadLine();
            string[] splitline = readline.Split(',');
            riverpset.PatchNum = Convert.ToInt32(splitline[1]);
            riverpset.Patchs = new PATCH[riverpset.PatchNum];

            riverbuffer.aNum = ConstVar._MAX_ARCNUM_aMap;
            riverbuffer.arcs = new ARC[riverbuffer.aNum];
            int PatchNo = -1;
            int ArcNo = -1;

            string Flag = string.Empty;
            int pointindex = 0;
            int inindex = -1;
            for (; ; )
            {
                readline = sr.ReadLine();
                if (readline.Trim() == "END") break;

                splitline = readline.Split(',');
                if (splitline[0] == "OUT")//多边形外环
                {
                    Flag = "OUT";
                    pointindex = 0;
                    inindex = -1;
                    ArcNo++;
                    PatchNo++;
                    int pntnum = Convert.ToInt32(splitline[1]);
                    riverbuffer.arcs[ArcNo].pNum = pntnum;
                    riverbuffer.arcs[ArcNo].pts = new PNT[riverbuffer.arcs[ArcNo].pNum];
                    riverpset.Patchs[PatchNo].oNum = 1;
                    riverpset.Patchs[PatchNo].oNo = ArcNo;

                    continue;
                }
                else if (splitline[0] == "IN")
                {
                    Flag = "IN";

                    riverpset.Patchs[PatchNo].iNum = Convert.ToInt32(splitline[1]);
                    riverpset.Patchs[PatchNo].iNo = new int[riverpset.Patchs[PatchNo].iNum];

                }
                else if (splitline[0] == "INO")
                {
                    inindex++;
                    ArcNo++;
                    pointindex = 0;
                    int pntnum = Convert.ToInt32(splitline[1]);
                    riverbuffer.arcs[ArcNo].pNum = pntnum;
                    riverbuffer.arcs[ArcNo].pts = new PNT[riverbuffer.arcs[ArcNo].pNum];
                    riverpset.Patchs[PatchNo].iNo[inindex] = ArcNo;

                }
                else
                {
                    if (Flag == "OUT")
                    {
                        riverbuffer.arcs[ArcNo].pts[pointindex].x = Convert.ToDouble(splitline[0]);
                        riverbuffer.arcs[ArcNo].pts[pointindex].y = Convert.ToDouble(splitline[1]);
                        pointindex++;
                    }
                    else if (Flag == "IN")
                    {

                        riverbuffer.arcs[ArcNo].pts[pointindex].x = Convert.ToDouble(splitline[0]);
                        riverbuffer.arcs[ArcNo].pts[pointindex].y = Convert.ToDouble(splitline[1]);
                        pointindex++;
                    }
                }
            }

            //独立遍历每个多边形
            MAPARC[] CentetLines = new MAPARC[riverpset.PatchNum];  //存储中心线结果




            //逐个多边形进行遍历
            

            double size = 1;
            for (int ii = 0; ii < riverpset.PatchNum; ii++)
            {
                
                CentetLines[ii].aNum = 0;
                CentetLines[ii].arcs = new ARC[1000];

                MAPARC tmpbuffer = new MAPARC();//
                tmpbuffer.Tol = size;

                PATCHSET tmpset = new PATCHSET();

                //将每个面依次作为一个图层输入
                //线的数量等于外环+内环
                tmpbuffer.aNum = riverpset.Patchs[ii].oNum + riverpset.Patchs[ii].iNum;
                tmpbuffer.arcs = new ARC[tmpbuffer.aNum];
                tmpset.PatchNum = 1;
                tmpset.Patchs = new PATCH[tmpset.PatchNum];
                tmpset.Patchs[0].oNum = riverpset.Patchs[ii].oNum;
                tmpset.Patchs[0].oNo = riverpset.Patchs[ii].oNo;
                tmpset.Patchs[0].iNum = riverpset.Patchs[ii].iNum;
                tmpset.Patchs[0].iNo = new int[tmpset.Patchs[0].iNum];

                tmpbuffer.arcs[0].pNum = riverbuffer.arcs[riverpset.Patchs[ii].oNo].pNum;
                tmpbuffer.arcs[0].pts = new PNT[tmpbuffer.arcs[0].pNum];
                for (int jj = 0; jj < tmpbuffer.arcs[0].pNum; jj++)
                {
                    tmpbuffer.arcs[0].pts[jj].x = riverbuffer.arcs[riverpset.Patchs[ii].oNo].pts[jj].x;
                    tmpbuffer.arcs[0].pts[jj].y = riverbuffer.arcs[riverpset.Patchs[ii].oNo].pts[jj].y;
                }

                for (int jj = 0; jj < riverpset.Patchs[ii].iNum; jj++)
                {
                    tmpset.Patchs[0].iNo[jj] = riverpset.Patchs[ii].iNo[jj];
                    tmpbuffer.arcs[jj + 1].pNum = riverbuffer.arcs[riverpset.Patchs[ii].iNo[jj]].pNum;
                    tmpbuffer.arcs[jj + 1].pts = new PNT[tmpbuffer.arcs[jj + 1].pNum];

                    for (int kk = 0; kk < riverbuffer.arcs[riverpset.Patchs[ii].iNo[jj]].pNum; kk++)
                    {
                        tmpbuffer.arcs[jj + 1].pts[kk].x = riverbuffer.arcs[riverpset.Patchs[ii].iNo[jj]].pts[kk].x;
                        tmpbuffer.arcs[jj + 1].pts[kk].y = riverbuffer.arcs[riverpset.Patchs[ii].iNo[jj]].pts[kk].y;
                    }
                }

                MapBoundInitial(ref tmpbuffer);



                tmpset.PatchNum = 1;
                tmpset.Patchs = new PATCH[tmpset.PatchNum];
                tmpset.RemoveSELF = false;
                tmpset.Patchs[0].oNum = 1;
                tmpset.Patchs[0].oNo = 0;
                tmpset.Patchs[0].iNum = tmpbuffer.aNum - 1;
                tmpset.Patchs[0].iNo = new int[tmpset.Patchs[0].iNum];
                for (int jj = 1; jj < tmpbuffer.aNum; jj++)
                {
                    tmpset.Patchs[0].iNo[jj - 1] = jj;
                }

                RASTER_PAR CELLs0 = new RASTER_PAR();
                GetRasterPara_BufARCS(ref tmpbuffer, ref CELLs0);

                /*
                for (int m =0; m< CELLs0.LINEs; m++)
                {
                    for(int n =0;n< CELLs0.COLs;n++)
                    {
                        image[m,n] = 0;
                        RemoveLabel[m, n] = false;

                    }
                }
                */

                //Byte[,] image = new Byte[11000, 11000];
                Byte[,] image = new Byte[CELLs0.LINEs, CELLs0.COLs];
                //Byte[,] RemoveLabel = new Byte[11000, 11000];
                Byte[,] RemoveLabel = new Byte[CELLs0.LINEs, CELLs0.COLs];
                //栅格矢量化
                //Byte[,] IsTraced = new Byte[11000, 11000];//标记某点是否被追

                for (int m = 0; m < CELLs0.LINEs; m++)
                {
                    for (int n = 0; n < CELLs0.COLs; n++)
                    {
                        image[m, n] = 0;
                        RemoveLabel[m, n] = 0;

                    }
                }
                PatchSet_To_Raster(ref tmpbuffer, ref tmpset, ref image, ref CELLs0);


                //---------------------------核心函数-----------------------

                int loop = 1;
                bool Changed = false;
                
                

                for (; ; )
                {
                    //按照行扫描网格
                    for (int m = 1; m < CELLs0.LINEs - 1; m++)
                    {
                        for (int n = 1; n < CELLs0.COLs - 1; n++)
                        {

                            // p9 p2 p3
                            // p8 p1 p4
                            // p7 p6 p5
                            int p1 = image[m, n];
                            if (p1 != 1) continue;
                            int p2 = image[m + 1, n];
                            int p3 = image[m + 1, n + 1];
                            int p4 = image[m, n + 1];
                            int p5 = image[m - 1, n + 1];
                            int p6 = image[m - 1, n];
                            int p7 = image[m - 1, n - 1];
                            int p8 = image[m, n - 1];
                            int p9 = image[m + 1, n - 1];

                            if ((p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9) >= 2 && (p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9) <= 6)
                            {
                                int ap = 0;
                                if (p2 == 0 && p3 == 1) ++ap;
                                if (p3 == 0 && p4 == 1) ++ap;
                                if (p4 == 0 && p5 == 1) ++ap;
                                if (p5 == 0 && p6 == 1) ++ap;
                                if (p6 == 0 && p7 == 1) ++ap;
                                if (p7 == 0 && p8 == 1) ++ap;
                                if (p8 == 0 && p9 == 1) ++ap;
                                if (p9 == 0 && p2 == 1) ++ap;
                                if (ap == 1)
                                {
                                    if (p2 * p4 * p6 == 0)
                                    {
                                        if (p4 * p6 * p8 == 0)
                                        {
                                            //标记  
                                            RemoveLabel[m, n] = 1;
                                        }
                                    }
                                }
                            }

                        }
                    }


                    for (int i = 1; i < CELLs0.LINEs - 1; i++)
                    {
                        for (int j = 1; j < CELLs0.COLs - 1; j++)
                        {
                            if (RemoveLabel[i, j] == 1)
                            {
                                image[i, j] = 0;

                                RemoveLabel[i, j] = 0;
                                Changed = true;
                            }


                        }
                    }

                    if (Changed == false)//无法继续剥离
                    {
                        break;
                    }
                    else
                    {
                        Changed = false;
                    }

                    //按照行扫描网格
                    for (int m = 1; m < CELLs0.LINEs - 1; m++)
                    {
                        for (int n = 1; n < CELLs0.COLs - 1; n++)
                        {
                            // p9 p2 p3
                            // p8 p1 p4
                            // p7 p6 p5
                            int p1 = image[m, n];
                            if (p1 != 1) continue;
                            int p2 = image[m + 1, n];
                            int p3 = image[m + 1, n + 1];
                            int p4 = image[m, n + 1];
                            int p5 = image[m - 1, n + 1];
                            int p6 = image[m - 1, n];
                            int p7 = image[m - 1, n - 1];
                            int p8 = image[m, n - 1];
                            int p9 = image[m + 1, n - 1];

                            if ((p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9) >= 2 && (p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9) <= 6)
                            {
                                int ap = 0;
                                if (p2 == 0 && p3 == 1) ++ap;
                                if (p3 == 0 && p4 == 1) ++ap;
                                if (p4 == 0 && p5 == 1) ++ap;
                                if (p5 == 0 && p6 == 1) ++ap;
                                if (p6 == 0 && p7 == 1) ++ap;
                                if (p7 == 0 && p8 == 1) ++ap;
                                if (p8 == 0 && p9 == 1) ++ap;
                                if (p9 == 0 && p2 == 1) ++ap;
                                if (ap == 1)
                                {
                                    if (p2 * p4 * p8 == 0)
                                    {
                                        if (p2 * p6 * p8 == 0)
                                        {
                                            //标记  
                                            RemoveLabel[m, n] = 1;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    for (int i = 1; i < CELLs0.LINEs - 1; i++)
                    {
                        for (int j = 1; j < CELLs0.COLs - 1; j++)
                        {
                            if (RemoveLabel[i, j] == 1)
                            {
                                image[i, j] = 0;

                                RemoveLabel[i, j] = 0;
                                Changed = true;
                            }


                        }
                    }

                    if (Changed == false)//无法继续剥离
                    {
                        break;
                    }
                    else
                    {
                        Changed = false;
                    }

                    loop++;

                }
                RemoveLabel = null;//释放空间
                /*
                //输出所有栅格点
                if(ii==0)
                {
                    List<IPoint> pntCol = new List<IPoint>();


                    for (int i = 1; i < CELLs0.LINEs - 1; i++)
                    {
                        for (int j = 1; j < CELLs0.COLs - 1; j++)
                        {
                            if (image[i, j] == 1)
                            {
                                IPoint pnt = new Point();
                                pnt.X = CELLs0.xmin + CELLs0.dxy * (j + 0.5);
                                pnt.Y = CELLs0.ymin + CELLs0.dxy * (i + 0.5);
                                pntCol.Add(pnt);
                            }
                        }
                    }
                    Feature2Shapefile(CenterLineDir, "zxx.shp", ref pntCol);
                }
                return;
                */


                //bool[,] KeyPoint = new bool[CELLs0.LINEs, CELLs0.COLs];//标记某点是不是关键点，不要删除
                /*
                for (int m = 0; m < CELLs0.LINEs; m++)
                {
                    for (int n = 0; n < CELLs0.COLs; n++)
                    {
                        IsTraced[m, n] = false;
                        //KeyPoint[m, n] = false;
                    }
                }
                */
                Byte[,] IsTraced = new Byte[CELLs0.LINEs, CELLs0.COLs];//标记某点是否被追
                for (int m = 0; m < CELLs0.LINEs; m++)
                {
                    for (int n = 0; n < CELLs0.COLs; n++)
                    {
                        IsTraced[m, n] = 0;

                    }
                }
                Changed = false;
                for (; ; )
                {


                    //按照行扫描网格
                    for (int m = 1; m < CELLs0.LINEs - 1; m++)
                    {

                        for (int n = 1; n < CELLs0.COLs - 1; n++)
                        {


                            if ((IsTraced[m, n] == 0) && (image[m, n] == 1))
                            {
                                //找起始点
                                int nextp = -1;
                                int multi4out = 0;// 标记那些有用上下左右四个出口中多个出口的交叉点
                                int ninecell = PointType(m, n, ref IsTraced, ref image, ref nextp, ref multi4out); //出口

                                if (ninecell != 1) continue;


                                Changed = true;

                                int tracei = m;
                                int tracej = n;

                                int prei = tracei;
                                int prej = tracej;


                                //int PT = PointType(tracei, tracej, ref IsTraced, ref image);

                                IsTraced[tracei, tracej] = 1;


                                ARC pArc = new ARC();
                                pArc.pNum = 0;
                                pArc.pts = new PNT[ConstVar._MAX_ARCNUM_aMap];
                                bool[] SelectID = new bool[ConstVar._MAX_ARCNUM_aMap];
                                for (int id = 0; id < ConstVar._MAX_ARCNUM_aMap; id++)
                                {
                                    SelectID[id] = false;
                                }
                                pArc.pts[pArc.pNum].x = CELLs0.xmin + CELLs0.dxy * (tracej + 0.5);
                                pArc.pts[pArc.pNum].y = CELLs0.ymin + CELLs0.dxy * (tracei + 0.5);
                                SelectID[pArc.pNum] = true;
                                pArc.pNum++;

                                for (; ; )
                                {
                                    //上方栅格
                                    if (nextp == 0)
                                    {

                                        prei = tracei;
                                        prej = tracej;
                                        tracei = tracei + 1;
                                        IsTraced[tracei, tracej] = 1;

                                        pArc.pts[pArc.pNum].x = CELLs0.xmin + CELLs0.dxy * (tracej + 0.5);
                                        pArc.pts[pArc.pNum].y = CELLs0.ymin + CELLs0.dxy * (tracei + 0.5);
                                        SelectID[pArc.pNum] = true;
                                        pArc.pNum++;
                                    }
                                    //右侧栅格
                                    else if (nextp == 2)
                                    {
                                        prei = tracei;
                                        prej = tracej;
                                        tracej = tracej + 1;
                                        IsTraced[tracei, tracej] = 1;

                                        pArc.pts[pArc.pNum].x = CELLs0.xmin + CELLs0.dxy * (tracej + 0.5);
                                        pArc.pts[pArc.pNum].y = CELLs0.ymin + CELLs0.dxy * (tracei + 0.5);
                                        SelectID[pArc.pNum] = true;
                                        pArc.pNum++;
                                    }
                                    //下方栅格
                                    else if (nextp == 4)
                                    {
                                        prei = tracei;
                                        prej = tracej;
                                        tracei = tracei - 1;
                                        IsTraced[tracei, tracej] = 1;

                                        pArc.pts[pArc.pNum].x = CELLs0.xmin + CELLs0.dxy * (tracej + 0.5);
                                        pArc.pts[pArc.pNum].y = CELLs0.ymin + CELLs0.dxy * (tracei + 0.5);
                                        SelectID[pArc.pNum] = true;
                                        pArc.pNum++;
                                    }
                                    //左侧栅格
                                    else if (nextp == 6)
                                    {
                                        prei = tracei;
                                        prej = tracej;
                                        tracej = tracej - 1;
                                        IsTraced[tracei, tracej] = 1;

                                        pArc.pts[pArc.pNum].x = CELLs0.xmin + CELLs0.dxy * (tracej + 0.5);
                                        pArc.pts[pArc.pNum].y = CELLs0.ymin + CELLs0.dxy * (tracei + 0.5);
                                        SelectID[pArc.pNum] = true;
                                        pArc.pNum++;
                                    }
                                    //右上
                                    else if (nextp == 1)
                                    {
                                        prei = tracei;
                                        prej = tracej;
                                        tracei = tracei + 1;
                                        tracej = tracej + 1;
                                        IsTraced[tracei, tracej] = 1;

                                        pArc.pts[pArc.pNum].x = CELLs0.xmin + CELLs0.dxy * (tracej + 0.5);
                                        pArc.pts[pArc.pNum].y = CELLs0.ymin + CELLs0.dxy * (tracei + 0.5);
                                        SelectID[pArc.pNum] = true;
                                        pArc.pNum++;
                                    }
                                    //右下
                                    else if (nextp == 3)
                                    {
                                        prei = tracei;
                                        prej = tracej;
                                        tracei = tracei - 1;
                                        tracej = tracej + 1;
                                        IsTraced[tracei, tracej] = 1;

                                        pArc.pts[pArc.pNum].x = CELLs0.xmin + CELLs0.dxy * (tracej + 0.5);
                                        pArc.pts[pArc.pNum].y = CELLs0.ymin + CELLs0.dxy * (tracei + 0.5);
                                        SelectID[pArc.pNum] = true;
                                        pArc.pNum++;
                                    }

                                    //左下
                                    else if (nextp == 5)
                                    {
                                        prei = tracei;
                                        prej = tracej;
                                        tracei = tracei - 1;
                                        tracej = tracej - 1;
                                        IsTraced[tracei, tracej] = 1;

                                        pArc.pts[pArc.pNum].x = CELLs0.xmin + CELLs0.dxy * (tracej + 0.5);
                                        pArc.pts[pArc.pNum].y = CELLs0.ymin + CELLs0.dxy * (tracei + 0.5);
                                        SelectID[pArc.pNum] = true;
                                        pArc.pNum++;
                                    }

                                    //左上栅格
                                    else if (nextp == 7)
                                    {
                                        prei = tracei;
                                        prej = tracej;
                                        tracei = tracei + 1;
                                        tracej = tracej - 1;
                                        IsTraced[tracei, tracej] = 1;

                                        pArc.pts[pArc.pNum].x = CELLs0.xmin + CELLs0.dxy * (tracej + 0.5);
                                        pArc.pts[pArc.pNum].y = CELLs0.ymin + CELLs0.dxy * (tracei + 0.5);
                                        SelectID[pArc.pNum] = true;
                                        pArc.pNum++;
                                    }

                                    else //如果找不到能够跟踪的点，则终止本弧追踪
                                    {
                                        break;
                                    }

                                    //查找当前点的下一出口 如果找到端点  
                                    nextp = -1;
                                    multi4out = 0;
                                    ninecell = PointType(tracei, tracej, ref IsTraced, ref image, ref nextp, ref multi4out);
                                    if (ninecell == 0)
                                    {
                                        break;
                                    }
                                    /*
                                    if(multi4out>1)
                                    {
                                        IsTraced[tracei, tracej] = 0;//如果为交叉点，弄成0，后面还要继续追踪
                                    }
                                    */
                                }

                                //
                                //if(pArc.pNum* tmpbuffer.Tol>50)
                                //{


                                ARC_ARJ(ref pArc, ref CentetLines[ii].arcs[CentetLines[ii].aNum++], -1, ref SelectID);
                                //}

                            }


                        }
                    }

                    if (Changed == false)
                    {
                        break;
                    }
                    else
                    {
                        Changed = false;
                    }

                }


            }

            
            //输出测试
            //Feature2Shapefile(CenterLineDir, "zxx.shp", ref CentetLines, riverpset.PatchNum);

        }
        bool InsertPointL(ref R_PNT InsertP, ref FLOAT_RL_LINE[] RL_GRID0,FLOAT_R_LINE[] R_GRID)
        {
            bool Changed = false;
            int p2 = GetValue_GRID(InsertP.y + 1, InsertP.x, ref RL_GRID0);
            int p3 = GetValue_GRID(InsertP.y + 1, InsertP.x + 1, ref RL_GRID0);
            int p4 = GetValue_GRID(InsertP.y, InsertP.x + 1, ref RL_GRID0);
            int p5 = GetValue_GRID(InsertP.y - 1, InsertP.x + 1, ref RL_GRID0);
            int p6 = GetValue_GRID(InsertP.y - 1, InsertP.x, ref RL_GRID0);
            int p7 = GetValue_GRID(InsertP.y - 1, InsertP.x - 1, ref RL_GRID0);
            int p8 = GetValue_GRID(InsertP.y, InsertP.x - 1, ref RL_GRID0);
            int p9 = GetValue_GRID(InsertP.y + 1, InsertP.x - 1, ref RL_GRID0);
            if ((p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9) >= 2 && (p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9) <= 6)
            {
                int ap = 0;
                if (p2 == 0 && p3 == 1) ++ap;
                if (p3 == 0 && p4 == 1) ++ap;
                if (p4 == 0 && p5 == 1) ++ap;
                if (p5 == 0 && p6 == 1) ++ap;
                if (p6 == 0 && p7 == 1) ++ap;
                if (p7 == 0 && p8 == 1) ++ap;
                if (p8 == 0 && p9 == 1) ++ap;
                if (p9 == 0 && p2 == 1) ++ap;
                if (ap == 1)
                {
                    if (p2 * p4 * p6 == 0)
                    {
                        if (p4 * p6 * p8 == 0)
                        {
                            Changed = true;
                            InsertR_Float(InsertP.y, InsertP.x, ref R_GRID);
                        }

                    }

                }

            }
            return Changed;
        }
        bool InsertPointR(ref R_PNT InsertP, ref FLOAT_RL_LINE[] RL_GRID0, FLOAT_R_LINE[] R_GRID)
        {
            bool Changed = false;
            int p2 = GetValue_GRID(InsertP.y + 1, InsertP.x, ref RL_GRID0);
            int p3 = GetValue_GRID(InsertP.y + 1, InsertP.x + 1, ref RL_GRID0);
            int p4 = GetValue_GRID(InsertP.y, InsertP.x + 1, ref RL_GRID0);
            int p5 = GetValue_GRID(InsertP.y - 1, InsertP.x + 1, ref RL_GRID0);
            int p6 = GetValue_GRID(InsertP.y - 1, InsertP.x, ref RL_GRID0);
            int p7 = GetValue_GRID(InsertP.y - 1, InsertP.x - 1, ref RL_GRID0);
            int p8 = GetValue_GRID(InsertP.y, InsertP.x - 1, ref RL_GRID0);
            int p9 = GetValue_GRID(InsertP.y + 1, InsertP.x - 1, ref RL_GRID0);
            if ((p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9) >= 2 && (p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9) <= 6)
            {
                int ap = 0;
                if (p2 == 0 && p3 == 1) ++ap;
                if (p3 == 0 && p4 == 1) ++ap;
                if (p4 == 0 && p5 == 1) ++ap;
                if (p5 == 0 && p6 == 1) ++ap;
                if (p6 == 0 && p7 == 1) ++ap;
                if (p7 == 0 && p8 == 1) ++ap;
                if (p8 == 0 && p9 == 1) ++ap;
                if (p9 == 0 && p2 == 1) ++ap;
                if (ap == 1)
                {
                    if (p2 * p4 * p8 == 0)
                    {
                        if (p2 * p6 * p8 == 0)
                        {
                            Changed = true;
                            InsertR_Float(InsertP.y, InsertP.x, ref R_GRID);
                        }

                    }

                }

            }
            return Changed;
        }
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
                        /*
                        //需要考虑这个游程只有一个网格的情况
                        if((C == (int)(RL_GRID[L].Line_RLs[i1].C1 - 1))&&(C == (int)(RL_GRID[L].Line_RLs[i1].C2)))
                        {
                            Type = 3; SecNo = i1; return true;
                        }
                        */
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
        int GetValue_GRID(int line, int col, ref FLOAT_RL_LINE[] RL_GRID0)
        {
            if (RL_GRID0[line].rl_Num == 0) return 0;
            else
            {
                for(int i=0;i< RL_GRID0[line].rl_Num;i++)
                {
                    if(RL_GRID0[line].Line_RLs[i].C1<=col&& RL_GRID0[line].Line_RLs[i].C2>=col)
                    {
                        return 1;
                    }
                }
            }
            return 0;
        }
        int GetValue_GRID(int line, int col, ref FLOAT_RL_LINE[] RL_GRID0, ref int p)
        {
            if (RL_GRID0[line].rl_Num == 0) return 0;
            else
            {
                for (int i = 0; i < RL_GRID0[line].rl_Num; i++)
                {
                    if (RL_GRID0[line].Line_RLs[i].C1 <= col && RL_GRID0[line].Line_RLs[i].C2 >= col)
                    {
                        p = i;
                        return 1;
                    }
                }
            }
            return 0;
        }
        void PatchSet_To_RLTable(ref MAPARC map, ref PATCHSET pset, ref FLOAT_RL_LINE[] RL_GRID0, ref RASTER_PAR CELLs0, bool OverlayID)
        //面的栅格化
        //参数：map记录弧集合
        //      pset记录面的弧段构成信息
        //      CELLs0表示栅格场参数
        //      RL_GRID0 表示栅格场
        //      OverlayID 表示是否是叠置, false 表示不是叠置，而是栅格化为一个全新场；
        //                true表示将结果叠置到一个已有的栅格场中
        {
            //1. 变量定义与初始化
            int i, j, Off_L, Off_C, aNo, aNum, L1, L2, L;
            double x, y, ymin, ymax, C1, C2;
            PNT P1, P2;
            ARC Arc;
            if (OverlayID == false)
            {
                for (i = 0; i < CELLs0.LINEs; i++)
                {
                    RL_GRID0[i] = new FLOAT_RL_LINE();
                    RL_GRID0[i].rl_Num = 0;
                    RL_GRID0[i].Line_RLs = new List<FLOAT_RL_LinkTable>();
                }
            }

            RASTER_PAR CELLs = new RASTER_PAR();

            //if (OverlayID == false)
            for (i = 0; i < map.aNum; i++) SetArcSpatialAtt(ref map.arcs[i]);

            //2. 对每个面进行栅格化
            for (int PatchNo = 0; PatchNo < pset.PatchNum; PatchNo++)
            {
                //2.1 计算当前面的栅格场参数
                CELLs.xmin = CELLs0.xmin;
                CELLs.dxy = CELLs0.dxy;
                Off_C = 0;
                //(1) 当前面无外边界时，采用总场的栅格参数
                if (pset.Patchs[PatchNo].oNum == 0)
                {
                    CELLs.ymin = CELLs0.ymin;
                    Off_L = 0;
                    CELLs.LINEs = CELLs0.LINEs;
                }
                //(2) 当前面有外边界时，根据外边界弧的范围计算栅格参数
                else
                {
                    ymin = 1E+256; ymax = -1E+256;
                    Arc = map.arcs[pset.Patchs[PatchNo].oNo];
                    if (Arc.ymin < ymin) ymin = Arc.ymin;
                    if (Arc.ymax > ymax) ymax = Arc.ymax;
                    L1 = GetIY(ref CELLs0, ymin) - 5; if (L1 < 0) L1 = 0;
                    L2 = GetIY(ref CELLs0, ymax) + 5; if (L2 >= CELLs0.LINEs) L2 = CELLs0.LINEs - 1;
                    CELLs.ymin = L1 * CELLs0.dxy + CELLs0.ymin;
                    CELLs.LINEs = L2 - L1;
                    Off_L = L1;
                }
                FLOAT_R_LINE[] R_GRID = new FLOAT_R_LINE[CELLs.LINEs];
                for (i = 0; i < CELLs.LINEs; i++)
                {
                    R_GRID[i] = new FLOAT_R_LINE();
                    R_GRID[i].rl_Num = 0;
                    R_GRID[i].Line_Rs = new List<FLOAT_R_LinkTable>();
                }

                //2.2 对当前面计算所有边界线与Y扫描线上的断点
                if (pset.Patchs[PatchNo].oNum == 0)
                {
                    for (L = 0; L < CELLs0.LINEs; L++)
                    {
                        InsertR_Float(L, 0, ref R_GRID);
                        InsertR_Float(L, CELLs0.COLs, ref R_GRID);
                    }
                }
                aNum = pset.Patchs[PatchNo].oNum + pset.Patchs[PatchNo].iNum;
                for (i = 0; i < aNum; i++)
                {
                    if (pset.Patchs[PatchNo].oNum == 1)
                    {
                        if (i == 0) aNo = pset.Patchs[PatchNo].oNo;
                        else aNo = pset.Patchs[PatchNo].iNo[i - 1];
                    }
                    else aNo = pset.Patchs[PatchNo].iNo[i];

                    Arc = map.arcs[aNo];
                    for (j = 0; j < Arc.pNum - 1; j++)
                    {
                        if (Arc.pts[j].y < Arc.pts[j + 1].y) { P1 = Arc.pts[j]; P2 = Arc.pts[j + 1]; }
                        else { P1 = Arc.pts[j + 1]; P2 = Arc.pts[j]; }
                        for (L = GetIY(ref CELLs, P1.y); L <= GetIY(ref CELLs, P2.y); L++)
                        {
                            

                            y = (L + 0.500123) * CELLs.dxy + CELLs.ymin;
                            if (y >= P1.y && y <= P2.y)
                            {
                                x = CrossX(P2, P1, y);
                                InsertR_Float(L, (float)((x - CELLs.xmin) / CELLs.dxy), ref R_GRID);
                            }
                        }
                    }
                }
                //2.3 根据各行的断点，插入总场

                for (i = 0; i < CELLs.LINEs; i++) if (R_GRID[i].rl_Num >= 2)
                    {
                        for (j = 0; j < R_GRID[i].rl_Num - 1; j += 2)
                        {
                            C1 = R_GRID[i].Line_Rs[j].C1;
                            C2 = R_GRID[i].Line_Rs[j + 1].C1;
                            InsertRL_Float(i + Off_L, (int)(C1 + Off_C), (int)(C2 + Off_C), ref RL_GRID0);
                        }
                    }

                //清除游程缝隙
                Clear_TinyGap(ref RL_GRID0, ref CELLs);
            }

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
        void RemoveRL_Float(int L, int C1, int C2, ref FLOAT_RL_LINE[] RL_GRID)
        //游程减法：从栅格场RL_GRID中第L行减去游程[C1,C2]
        {

            if (RL_GRID[L].rl_Num <= 0) return;          //当前行内还没有任何游程的情形
            else if (RL_GRID[L].Line_RLs[0].C1 > C2) return;  //在第一个游程前面
            else if (RL_GRID[L].Line_RLs[RL_GRID[L].rl_Num - 1].C2 < C1) return;    //在最后游程后面
                                                                                    //其他情形, 先分析C1，C2的位置情况

            int CASE1 = 0, CASE2 = 0, II, I1 = -1, I2 = -2;
            //找出C1在哪个游程段之前(CASE=-1)或之内(CASE=1)
            for (II = 0; II < RL_GRID[L].rl_Num; II++)
            {
                if (C1 < RL_GRID[L].Line_RLs[II].C1) { CASE1 = -1; I1 = II; break; }
                else if (C1 <= RL_GRID[L].Line_RLs[II].C2) { CASE1 = 1; I1 = II; break; }
            }
            //找出C2在哪个游程段之前(CASE=-1)或之内(CASE=1),或在最末段之后(CASe=2)
            if (C2 > RL_GRID[L].Line_RLs[RL_GRID[L].rl_Num - 1].C2) CASE2 = 2;
            else for (; II < RL_GRID[L].rl_Num; II++)
                {
                    if (C2 < RL_GRID[L].Line_RLs[II].C1) { CASE2 = -1; I2 = II; break; }
                    else if (C2 <= RL_GRID[L].Line_RLs[II].C2) { CASE2 = 1; I2 = II; break; }
                }
            //根据C1,C2的位置进行处理
            if (I1 == I2)
            {    //如果处于同一段
                if (CASE1 == 1 && CASE2 == 1)
                {
                    //考虑更复杂的情况  位于游程最左边
                    if (C1 == RL_GRID[L].Line_RLs[I1].C1)
                    {
                        if(C2 == RL_GRID[L].Line_RLs[I1].C2)
                        {
                            RL_GRID[L].Line_RLs.RemoveAt(I1);
                            RL_GRID[L].rl_Num--;
                        }

                        else
                        {
                            RL_GRID[L].Line_RLs[I1].C1= (C2+1);
                        }

                    }
                    else if (C2 == RL_GRID[L].Line_RLs[I1].C2) //最右边
                    {

                        RL_GRID[L].Line_RLs[I1].C2 = (C1-1);
                       
                    }
                    else
                    {
                        FLOAT_RL_LinkTable This = new FLOAT_RL_LinkTable();
                        This.C1 = C2 + 1; This.C2 = RL_GRID[L].Line_RLs[I1].C2; RL_GRID[L].rl_Num++;
                        RL_GRID[L].Line_RLs[I1].C2 = (C1 - 1);
                        RL_GRID[L].Line_RLs.Insert(I1 + 1, This);
                    }


                }
                else if (CASE1 == -1 && CASE2 == 1)
                {
                    if(C2== RL_GRID[L].Line_RLs[I1].C2)
                    {
                        RL_GRID[L].Line_RLs.RemoveAt(I1);
                        RL_GRID[L].rl_Num--;
                    }

                    else
                    {
                        RL_GRID[L].Line_RLs[I1].C1 = (C2 + 1);
                    }
                    
                }
            }
            else
            {
                if (CASE2 == 1)
                {
                    if (CASE1 == -1)
                    {
                        if(C2<RL_GRID[L].Line_RLs[I2].C2) //C2位于内部
                        {
                            RL_GRID[L].Line_RLs[I2].C1 = C2 + 1;

                            for (int I = I2 - 1; I >= I1; I--)
                            { RL_GRID[L].Line_RLs.RemoveAt(I); RL_GRID[L].rl_Num--; }
                        }
                        else 
                        {
                            for (int I = I2; I >= I1; I--)
                            { RL_GRID[L].Line_RLs.RemoveAt(I); RL_GRID[L].rl_Num--; }
                        }

                    }
                    else if (CASE1 == 1)
                    {
                        if(C2<RL_GRID[L].Line_RLs[I2].C2)
                        {
                            if(C1> RL_GRID[L].Line_RLs[I1].C1)
                            {
                                RL_GRID[L].Line_RLs[I2].C1 =( C2 + 1);
                                RL_GRID[L].Line_RLs[I1].C2 = (C1 - 1);
                                for (int I = I2 - 1; I >= I1 + 1; I--)
                                { RL_GRID[L].Line_RLs.RemoveAt(I); RL_GRID[L].rl_Num--; }
                            }
                            else
                            {
                                RL_GRID[L].Line_RLs[I2].C1 = (C2 + 1);
                                for (int I = I2 - 1; I >= I1; I--)
                                { RL_GRID[L].Line_RLs.RemoveAt(I); RL_GRID[L].rl_Num--; }
                            }                       
                        }
                        else
                        {
                            if (C1 > RL_GRID[L].Line_RLs[I1].C1)
                            {
                                RL_GRID[L].Line_RLs[I1].C2 = (C1 - 1);
                                for (int I = I2; I >I1; I--)
                                { RL_GRID[L].Line_RLs.RemoveAt(I); RL_GRID[L].rl_Num--; }
                            }
                            else
                            {
                                for (int I = I2; I >= I1; I--)
                                { RL_GRID[L].Line_RLs.RemoveAt(I); RL_GRID[L].rl_Num--; }
                            }
                        }
                    }
                }
                else if (CASE2 == 2)
                {
                    if (CASE1 == -1)
                    {
                        I2 = RL_GRID[L].rl_Num - 1;
                        for (int I = I2; I >= I1; I--)
                        { RL_GRID[L].Line_RLs.RemoveAt(I); RL_GRID[L].rl_Num--; }

                    }
                    else if (CASE1 == 1)
                    {
                        if(RL_GRID[L].Line_RLs[I1].C1 < C1)
                        {
                            RL_GRID[L].Line_RLs[I1].C2 = (C1-1);
                            I2 = RL_GRID[L].rl_Num - 1;
                            for (int I = I2; I >= I1 + 1; I--)
                            { RL_GRID[L].Line_RLs.RemoveAt(I); RL_GRID[L].rl_Num--; }
                        }
                        else 
                        {
                            I2 = RL_GRID[L].rl_Num - 1;
                            for (int I = I2; I >= I1; I--)
                            { RL_GRID[L].Line_RLs.RemoveAt(I); RL_GRID[L].rl_Num--; }
                        }
                        
                    }
                }
                else if (CASE2 == -1)
                {
                    if (CASE1 == -1)
                    {
                        for (int I = I2 - 1; I >= I1; I--)
                        { RL_GRID[L].Line_RLs.RemoveAt(I); RL_GRID[L].rl_Num--; }
                    }
                    else if (CASE1 == 1)
                    {
                        if (RL_GRID[L].Line_RLs[I1].C1 < C1)
                        {
                            RL_GRID[L].Line_RLs[I1].C2 =( C1-1);
                            for (int I =( I2 - 1); I >= I1 + 1; I--)
                            { RL_GRID[L].Line_RLs.RemoveAt(I); RL_GRID[L].rl_Num--; }
                        }
                        else 
                        {
                            for (int I =( I2 - 1); I >= I1; I--)
                            { RL_GRID[L].Line_RLs.RemoveAt(I); RL_GRID[L].rl_Num--; }
                        }
                            
                    }
                }
            }

        }
        void InsertRL_Float(int L, int C1, int C2, ref FLOAT_RL_LINE[] RL_GRID)
        //在游程链表RL_GRID表示的栅格场中，第L行插入实游程[C1,C2]
        {
            
            //如果咩有游程
            if (RL_GRID[L].rl_Num <= 0)
            {
                FLOAT_RL_LinkTable This = new FLOAT_RL_LinkTable();
                This.C1 = C1; This.C2 = C2; RL_GRID[L].Line_RLs.Insert(0, This); RL_GRID[L].rl_Num = 1;
                return;
            }
            //如果位于最前面
            else if (RL_GRID[L].Line_RLs[0].C1 > C2)
            {
                FLOAT_RL_LinkTable This = new FLOAT_RL_LinkTable();
                This.C1 = C1; This.C2 = C2;
                RL_GRID[L].Line_RLs.Insert(0, This); RL_GRID[L].rl_Num++;
                return;
            }
            //位于最后
            else if (RL_GRID[L].Line_RLs[RL_GRID[L].rl_Num - 1].C2 < C1)
            {
                FLOAT_RL_LinkTable This = new FLOAT_RL_LinkTable();
                This.C1 = C1; This.C2 = C2;
                RL_GRID[L].Line_RLs.Add(This); RL_GRID[L].rl_Num++;
                return;
            }
            //如果部分覆盖最后一个游程
            else if (C1 >= RL_GRID[L].Line_RLs[RL_GRID[L].rl_Num - 1].C1)
            {
                if (C2 > RL_GRID[L].Line_RLs[RL_GRID[L].rl_Num - 1].C2) RL_GRID[L].Line_RLs[RL_GRID[L].rl_Num - 1].C2 = C2; return;
            }
            //如果部分覆盖的一个游程
            else if (C2 <= RL_GRID[L].Line_RLs[0].C2)
            {
                if (C1 < RL_GRID[L].Line_RLs[0].C1) RL_GRID[L].Line_RLs[0].C1 = C1; return;
            }
            int CASE1 = 0, CASE2 = 0, II, I1 = -1, I2 = -2;
            //CASE1=-1表示位于某游程的左侧 I1标记了该游程
            //CASE1=1表示位于某游程的上侧 I1标记了该游程
            for (II = 0; II < RL_GRID[L].rl_Num; II++)
            {
                if (C1 < RL_GRID[L].Line_RLs[II].C1) { CASE1 = -1; I1 = II; break; }
                else if (C1 <= RL_GRID[L].Line_RLs[II].C2) { CASE1 = 1; I1 = II; break; }
            }
            if (C2 > RL_GRID[L].Line_RLs[RL_GRID[L].rl_Num - 1].C2) CASE2 = 2;
            //CASE2=-1表示位于某游程的左侧 I2标记了该游程
            //CASE2=1表示位于某游程的上侧 I2标记了该游程
            else
            {
                for (; II < RL_GRID[L].rl_Num; II++)
                {
                    if (C2 < RL_GRID[L].Line_RLs[II].C1) { CASE2 = -1; I2 = II; break; }
                    else if (C2 <= RL_GRID[L].Line_RLs[II].C2) { CASE2 = 1; I2 = II; break; }
                }
            }
            //C1 C2都跟某一游程有关
            if (I1 == I2)
            {
                if (CASE1 == -1 && CASE2 == -1)//都位于左侧 此时I1和I2相等
                {
                    FLOAT_RL_LinkTable This = new FLOAT_RL_LinkTable();
                    This.C1 = C1; This.C2 = C2; RL_GRID[L].Line_RLs.Insert(II, This); RL_GRID[L].rl_Num++;
                }
                else if (CASE1 == -1 && CASE2 == 1) RL_GRID[L].Line_RLs[II].C1 = C1;
            }
            else//C1和C2分别跟不同的游程有关
            {
                if (CASE2 != 1)//C2位于缝隙
                {
                    if (CASE1 == -1) RL_GRID[L].Line_RLs[I1].C1 = C1; RL_GRID[L].Line_RLs[I1].C2 = C2;
                    I2 = (CASE2 == 2) ? RL_GRID[L].rl_Num - 1 : I2 - 1;
                    for (int I = I2; I >= I1 + 1; I--)
                    {
                        RL_GRID[L].Line_RLs.RemoveAt(I);
                    }
                    RL_GRID[L].rl_Num -= I2 - I1;

                }
                else//C2在某游程上
                {
                    if (CASE1 == -1) RL_GRID[L].Line_RLs[I1].C1 = C1; RL_GRID[L].Line_RLs[I1].C2 = RL_GRID[L].Line_RLs[I2].C2;

                    for (int I = I2; I >= I1 + 1; I--)
                    {
                        RL_GRID[L].Line_RLs.RemoveAt(I);

                    }
                    RL_GRID[L].rl_Num -= I2 - I1;
                }
            }
        }
        
        int CrossPoint(int tracei, int tracej, ref Byte[,] image)
        {
            int OutletNum = 0;

            //上方栅格
            if (image[tracei + 1, tracej] == 1 )
            {
                OutletNum++;
            }
            //右侧栅格
            if (image[tracei, tracej + 1] == 1 )
            {
                OutletNum++;
            }
            //下方栅格
            if (image[tracei - 1, tracej] == 1)
            {
                OutletNum++;
            }
            //左侧栅格
            if (image[tracei, tracej - 1] == 1 )
            {
                OutletNum++;
            }
            //左上栅格
            if (image[tracei + 1, tracej - 1] == 1)
            {
                OutletNum++;
            }
            //右上
            if (image[tracei + 1, tracej + 1] == 1 )
            {
                OutletNum++;
            }
            //右下
            if (image[tracei - 1, tracej + 1] == 1 )
            {
                OutletNum++;
            }
            //左下
            if (image[tracei - 1, tracej - 1] == 1 )
            {
                OutletNum++;
            }
            return OutletNum;
        }
        int PointType(int tracei, int tracej, ref Byte[,] IsTraced, ref Byte[,] image, ref int nextp,ref int multi4out)
        {
            int OutletNum = 0;

            //上方栅格
            if (image[tracei + 1, tracej] == 1 && IsTraced[tracei + 1, tracej] == 0)
            {
                OutletNum++;
                multi4out++;
                
                if(nextp<0) nextp = 0;
            }
            //右侧栅格
            if (image[tracei, tracej + 1] == 1 && IsTraced[tracei, tracej + 1] == 0)
            {
                OutletNum++;
                multi4out++;

                if (nextp < 0) nextp = 2;
            }
            //下方栅格
            if (image[tracei - 1, tracej] == 1 && IsTraced[tracei - 1, tracej] == 0)
            {
                OutletNum++;
                multi4out++;

                if (nextp < 0) nextp = 4;
            }
            //左侧栅格
            if (image[tracei, tracej - 1] == 1 && IsTraced[tracei, tracej - 1] == 0)
            {
                OutletNum++;
                multi4out++;

                if (nextp < 0) nextp = 6;
            }
            //左上栅格
             if (image[tracei + 1, tracej - 1] == 1  && IsTraced[tracei + 1, tracej - 1] == 0)
            {
                OutletNum++;

                if (nextp < 0) nextp = 7;
            }
            //右上
             if (image[tracei + 1, tracej + 1] == 1 && IsTraced[tracei + 1, tracej + 1] == 0)
            {
                OutletNum++;
                if (nextp < 0) nextp = 1;
            }
            //右下
             if (image[tracei - 1, tracej + 1] == 1  && IsTraced[tracei - 1, tracej + 1] == 0)
            {
                OutletNum++;
                if (nextp < 0) nextp = 3;
            }
            //左下
             if (image[tracei - 1, tracej - 1] == 1  && IsTraced[tracei - 1, tracej - 1] == 0)
            {
                OutletNum++;
                if (nextp < 0) nextp = 5;
            }
            return OutletNum;
        }
        void ARC_ARJ(ref ARC Arc1, ref ARC Arc2, double Tol,ref bool[] SelectID)
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
            /*
            bool[] SelectID = new bool[Arc1.pNum];
            for (int i = 0; i < Arc1.pNum; i++)
            {
                SelectID[i] = false;
            }
            */
            int Mid = Arc1.pNum / 2; int SelectNum = 3;
            SelectID[0] = SelectID[Arc1.pNum - 1] = SelectID[Mid] = true;
            Compress(Arc1.pts, 0, Mid, Tol * Tol, SelectID, ref SelectNum);
            Compress(Arc1.pts, Mid, Arc1.pNum - 1, Tol * Tol, SelectID, ref SelectNum);
            Arc2.pNum = SelectNum;
            Arc2.pts = new PNT[Arc1.pNum];
            SelectNum = 0;
            for (int i = 0; i < Arc1.pNum; i++)
                if (SelectID[i]) Arc2.pts[SelectNum++] = Arc1.pts[i];
            //SelectID = null;
        }
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


        void MapBoundInitial(ref MAPARC map)
        //计算和设置弧类型图层的范围
        {
            map.xmin = map.ymin = 1E256;
            map.xmax = map.ymax = -1E256;
            for (int i = 0; i < map.aNum; i++)
            {
                SetArcSpatialAtt(ref map.arcs[i]);
                if (map.xmax < map.arcs[i].xmax) map.xmax = map.arcs[i].xmax;
                if (map.ymax < map.arcs[i].ymax) map.ymax = map.arcs[i].ymax;
                if (map.xmin > map.arcs[i].xmin) map.xmin = map.arcs[i].xmin;
                if (map.ymin > map.arcs[i].ymin) map.ymin = map.arcs[i].ymin;
            }
            map.dx = map.xmax - map.xmin;
            map.dy = map.ymax - map.ymin;
        }
        void SetArcSpatialAtt(ref ARC Arc)
        //计算并设置弧段范围（xmin,xmax,ymin,ymax）
        {
            double x, y, xmin, ymin, xmax, ymax;
            xmin = ymin = 1E256;
            xmax = ymax = -1E256;
            for (int i = 0; i < Arc.pNum; i++)
            {
                x = Arc.pts[i].x; y = Arc.pts[i].y;
                if (x > xmax) xmax = x; if (x < xmin) xmin = x;
                if (y > ymax) ymax = y; if (y < ymin) ymin = y;
            }
            Arc.xmax = xmax; Arc.xmin = xmin;
            Arc.ymax = ymax; Arc.ymin = ymin;
        }
        private void GetRasterPara_BufARCS(ref MAPARC ArcSet, ref RASTER_PAR CELLs)
        {
            //1. 计算缓冲结果的矩形范围
            double xmin = 1E+100, ymin = 1E100, xmax = -1E100, ymax = -1E100,
                   dx, dy, MaxDXY, xy_add;

            for (int i = 0; i < ArcSet.aNum; i++)
            {
                SetArcSpatialAtt(ref ArcSet.arcs[i]);

                if (ArcSet.arcs[i].xmin - ArcSet.R < xmin) xmin = ArcSet.arcs[i].xmin - ArcSet.R;
                if (ArcSet.arcs[i].xmax + ArcSet.R > xmax) xmax = ArcSet.arcs[i].xmax + ArcSet.R;
                if (ArcSet.arcs[i].ymin - ArcSet.R < ymin) ymin = ArcSet.arcs[i].ymin - ArcSet.R;
                if (ArcSet.arcs[i].ymax + ArcSet.R > ymax) ymax = ArcSet.arcs[i].ymax + ArcSet.R;
            }
            //throw new NotImplementedException();
            dx = xmax - xmin; dy = ymax - ymin; MaxDXY = (dx > dy) ? dx : dy;
            //2. 计算各网步长及格网数
            if (ArcSet.Tol > 0)    //用户指定Tol模式(Tol实际上是格网大小)
            {
                CELLs.dxy = ArcSet.Tol;
                //if (MaxDXY > ConstVar._MAX_RASTER_LCs * ArcSet.Tol) CELLs.dxy = MaxDXY / ConstVar._MAX_RASTER_LCs;
                //if (MaxDXY < ConstVar._MIN_RASTER_LCs * ArcSet.Tol) CELLs.dxy = MaxDXY / ConstVar._MIN_RASTER_LCs;
                //if (MaxDXY > 10000 * ArcSet.Tol) CELLs.dxy = MaxDXY / 10000;
                //if (MaxDXY < 10000 * ArcSet.Tol) CELLs.dxy = MaxDXY / 10000;
                //if (CELLs.dxy < 1) CELLs.dxy = 1;


                CELLs.xmin = xmin - 5 * CELLs.dxy;
                CELLs.xmax = xmax + 5 * CELLs.dxy;
                CELLs.ymin = ymin - 5 * CELLs.dxy;
                CELLs.ymax = ymax + 5 * CELLs.dxy;
                CELLs.dx = CELLs.xmax - CELLs.xmin;
                CELLs.dy = CELLs.ymax - CELLs.ymin;
            }
            else     //自动计算模式，格网尺度为最小缓冲距离的1/50.
            {
                double RMIN = 1E100;
                for (int i = 0; i < ArcSet.aNum; i++)
                    if (RMIN > ArcSet.R) RMIN = ArcSet.R;
                CELLs.dxy = RMIN / 50;
                if (MaxDXY > ConstVar._MAX_RASTER_LCs * CELLs.dxy) CELLs.dxy = MaxDXY / ConstVar._MAX_RASTER_LCs;
                if (MaxDXY < ConstVar._MIN_RASTER_LCs * CELLs.dxy) CELLs.dxy = MaxDXY / ConstVar._MIN_RASTER_LCs;
                CELLs.xmin = xmin - 5 * CELLs.dxy;
                CELLs.xmax = xmax + 5 * CELLs.dxy;
                CELLs.ymin = ymin - 5 * CELLs.dxy;
                CELLs.ymax = ymax + 5 * CELLs.dxy;
                CELLs.dx = CELLs.xmax - CELLs.xmin;
                CELLs.dy = CELLs.ymax - CELLs.ymin;
            }
            CELLs.LINEs = (int)(CELLs.dy / CELLs.dxy + 0.5);
            CELLs.COLs = (int)(CELLs.dx / CELLs.dxy + 0.5);
        }

        void PatchSet_To_Raster(ref MAPARC map, ref PATCHSET pset, ref Byte[,] pRasterGrids, ref RASTER_PAR CELLs0)
        //面的栅格化
        {
            //1. 变量定义与初始化
            int i, j, Off_L, Off_C, aNo, aNum, L1, L2, L;
            double x, y, ymin, ymax, C1, C2;
            PNT P1, P2;
            ARC Arc;

            FLOAT_R_LINE[] R_GRID = new FLOAT_R_LINE[CELLs0.LINEs];
            for (i = 0; i < CELLs0.LINEs; i++)
            {
                R_GRID[i] = new FLOAT_R_LINE();
                R_GRID[i].rl_Num = 0;
                R_GRID[i].Line_Rs = new List<FLOAT_R_LinkTable>();
            }


            //2. 对每个面进行栅格化
            for (int PatchNo = 0; PatchNo < pset.PatchNum; PatchNo++)
            {
                aNum = pset.Patchs[PatchNo].oNum + pset.Patchs[PatchNo].iNum;
                for (i = 0; i < aNum; i++)
                {
                    if (pset.Patchs[PatchNo].oNum == 1)
                    {
                        if (i == 0) aNo = pset.Patchs[PatchNo].oNo;
                        else aNo = pset.Patchs[PatchNo].iNo[i - 1];
                    }
                    else aNo = pset.Patchs[PatchNo].iNo[i];

                    Arc = map.arcs[aNo];
                    for (j = 0; j < Arc.pNum - 1; j++)
                    {
                        if (Arc.pts[j].y < Arc.pts[j + 1].y) { P1 = Arc.pts[j]; P2 = Arc.pts[j + 1]; }
                        else { P1 = Arc.pts[j + 1]; P2 = Arc.pts[j]; }
                        for (L = GetIY(ref CELLs0, P1.y); L <= GetIY(ref CELLs0, P2.y); L++)
                        {
                            y = (L + 0.500123) * CELLs0.dxy + CELLs0.ymin;
                            if (y >= P1.y && y <= P2.y)
                            {
                                x = CrossX(P2, P1, y);
                                InsertR_Float(L, (float)((x - CELLs0.xmin) / CELLs0.dxy), ref R_GRID);
                            }
                        }
                    }
                }
                //2.3 根据各行的断点，插入总场

                for (i = 0; i < CELLs0.LINEs; i++) if (R_GRID[i].rl_Num >= 2)
                    {
                        for (j = 0; j < R_GRID[i].rl_Num - 1; j += 2)
                        {
                            int left = (int)R_GRID[i].Line_Rs[j].C1;
                            int right = (int)R_GRID[i].Line_Rs[j + 1].C1;
                            for(int k= left; k<= right; k++)
                            {
                                pRasterGrids[i, k] = 1;
                            }
                            
                        }
                    }

            }

        }
        int GetIY(ref RASTER_PAR CELLs, double y)
        //根据栅格场参数，计算坐标 y 对应于栅格场的行号
        {
            return (int)((y - CELLs.ymin) / CELLs.dxy);
        }
        //---------------------------------------------------------------------------
        int GetIX(ref RASTER_PAR CELLs, double x)
        //根据栅格场参数，计算坐标 x 对应于栅格场的列号
        {
            return (int)((x - CELLs.xmin) / CELLs.dxy);
        }
        double CrossX(PNT A, PNT B, double y)
        //计算直线AB与横线y 之间交点的 x 坐标
        {
            //return (y - B.y) * (B.x - A.x) / (B.y - A.y + 1E256) + B.x;
            return (y - B.y) * (B.x - A.x) / (B.y - A.y) + B.x;
        }
        void InsertR_Float(int L, float C, ref FLOAT_R_LINE[] RL_GRID)
        // 向断点链表栅格场内第L行插入断点 C, 类似于"插入游程"函数
        {
            if (RL_GRID[L].rl_Num <= 0)
            {         //当前行内还没有任何游程的情形
                FLOAT_R_LinkTable This = new FLOAT_R_LinkTable();
                This.C1 = C; RL_GRID[L].rl_Num = 1;
                RL_GRID[L].Line_Rs.Add(This); return;
            }
            else if (RL_GRID[L].Line_Rs[0].C1 > C)
            { //插入的游程在第一个游程前面
                FLOAT_R_LinkTable This = new FLOAT_R_LinkTable();
                This.C1 = C;
                RL_GRID[L].Line_Rs.Insert(0, This); RL_GRID[L].rl_Num++; return;
            }
            else if (RL_GRID[L].Line_Rs[RL_GRID[L].rl_Num - 1].C1 < C)
            {   //插入的游程在最后游程后面
                FLOAT_R_LinkTable This = new FLOAT_R_LinkTable();
                This.C1 = C;
                RL_GRID[L].Line_Rs.Add(This); RL_GRID[L].rl_Num++; return;
            }
            //其他情形, 先分析C1，C2的位置情况

            int CASE1 = 1, II;
            //找出C1在哪个游程段之前(CASE=-1)或之内(CASE=1)
            for (II = 0; II < RL_GRID[L].rl_Num; II++)
            {
                //如果碰到等于的，直接返回
                if (C == RL_GRID[L].Line_Rs[II].C1) return;

                if (C < RL_GRID[L].Line_Rs[II].C1) { CASE1 = -1; break; }
            }
            //根据C1的位置进行处理
            if (CASE1 == -1)
            {
                FLOAT_R_LinkTable This = new FLOAT_R_LinkTable();
                This.C1 = C; RL_GRID[L].Line_Rs.Insert(II, This); RL_GRID[L].rl_Num++;
            }
        }
        
    }
}
