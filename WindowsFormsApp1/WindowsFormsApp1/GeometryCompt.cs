using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RasGIS.CoreFunctions
{
    

    public struct seedpoint
    {
        public string name;
        public double zvalue;
        public double X;
        public double Y;

    }

    //网格单元结构
    public struct C_Cell
    {
        public int x;
        public int y;
        public C_Cell(int dx, int dy)
        {
            x = dx;
            y = dy;
        }
    }
    public struct RASTER_PAR      //栅格场结构
    {
        public int LINEs, COLs;      //行列数
        public double xmin, xmax, ymin, ymax, dx, dy; //dx为X轴向的总距离,dy为Y轴向的总距离
        public double CellSizeX, CellSizeY;
        //坐标范围及跨度
        public double dxy;          //格网边长
    };

    public struct KeyPoint
    {
        int pointtype; //1 端点  2 连接点  0汇聚点

    }

    public class FLOAT_RL_LinkTable       //实游程链表
    {

        public int C1;
        public int C2;         //游程起点、终点

    };
    public class FLOAT_R_LinkTable               //断点链表(记录每一行内的断点)
    {
        public float C1;

    };
    public class FLOAT_R_LINE                    //一行的所有断点
    {
        public int rl_Num;                          //断点数
        public List<FLOAT_R_LinkTable> Line_Rs;       //该行的断点链表
    };
    /*
    public class RL_Node
    //实游程链表
    {
        public int C1, C2;         //游程起点、终点
        public bool V1, V2;

    };
    public class RL_LINE  //一行的游程链表信息
    {
        public int rl_Num;         //游程数量

        public List<RL_Node> RLS;

    };
    */
    public class FLOAT_RL_LINE  //一行的游程链表信息
    {
        public int rl_Num;         //游程数量
        public List<FLOAT_RL_LinkTable> Line_RLs;    //该行的游程链表, 从Begin一至链结到End
    };
    public struct PNT             //点坐标
    {
        public double x, y;
    };
    public struct ARC             //一条弧段
    {
        public int pNum;            //点数量
        public PNT[] pts;           //弧段的各点坐标
        public double xmax, xmin, ymax, ymin;//弧段范围

    };
    public struct MAPARC
    {        //弧类型图
        public int aNum;            //弧段数量
        public ARC[] arcs;          //弧段集合
        public double xmax, xmin, ymax, ymin, dx, dy;//全集的坐标范围
        public double R;//缓冲距离
        public double Tol;         //容许误差，实际上为计算格网大小
    };

    public struct PATCH           //面结构（面的构成弧段信息）
    {
        public int oNum;            //外边界数（1或0，表示有或无外边界）
        public int oNo;             //外边界弧段号
        public int iNum;            //内边界数
        public int[] iNo;            //内边界弧号

        public int SeedNo;
    };
    public struct PATCHSET        //面集合
    {
        public int PatchNum;        //集合内面的数量
        public PATCH[] Patchs;       //所有面的信息

        //面缓冲参数
        public bool RemoveSELF;     //扣除原面标识(true-扣除，false-不扣除)
        //int* Side;          //各个面的缓冲方向(0-双向,1-外侧,-1 内侧)
    };
    public struct R_PNT           //栅格场上的点（列号、行号）
    {
        public int x, y;  //表示行列号
    };
}
