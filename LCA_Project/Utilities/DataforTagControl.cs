using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using LiveCharts.Helpers;
namespace LCA_Project.Utilities
{
    public class DataforTagControl
    {
        public string LoadUnloadSocket { get; set; }
        public string RunUnloadSocket { get; set; }
        public string UnloadSocketX { get; set; }
        public string UnloadSocketY { get; set; }
        public string LimitX_ { get; set; }
        public string LimitX__ { get; set; }
        public string LimitY_ { get; set; }
        public string LimitY__ { get; set; }
        public string LimitZ_ { get; set; }
        public string LimitZ__ { get; set; }
        public string LimitF_ { get; set; }
        public string LimitF__ { get; set; }
        public string SensorORGX { get; set; }
        public string SensorORGY { get; set; }
        public string SensorORGZ { get; set; }
        public string SensorORGF { get; set; }
        public string StepJog { get; set; }
        #region Manual Switch 2
        public string SWsAuto2 { get; set; }
        public string SWsStep2 { get; set; }
        public string SWsHSp2 { get; set; }
        public string SWsStepPr2 { get; set; }
        public string SWsNextPr2 { get; set; }
        public string SWsPause2 { get; set; }
        public string SWsDiAb2 { get; set; }
        #endregion
        #region Manual Switch
        public string RqOrg { get; set; }
        public string RqReset { get; set; }
        public string RqMP0 { get; set; }
        public string RqMP1 { get; set; }
        public string RqMP2 { get; set; }
        public string RqMP3 { get; set; }
        public string RqMP4 { get; set; }
        public string RqMP5 { get; set; }
        public string RqMP6 { get; set; }
        public string RqMVFix { get; set; }
        public string RqOpenSK { get; set; }
        public string RqCloseSK { get; set; }
        public string RqVInt { get; set; }
        public string RqVOut { get; set; }
        public string RqVSocket { get; set; }
        public string RqXyLanhChart { get; set; }
        public string RqVChckSk2 { get; set; }
        public string RqVin2 { get; set; }
        public string RqVout2 { get; set; }
        public string RqMCOut { get; set; }
        public string RqMCDown { get; set; }
        public string RqMCClp { get; set; }
        public string RqMVCFix { get; set; }
        #endregion
        #region Axis X
        public string TimeOutX { get; set; }
        public string DisStep__JogX { get; set; }
        public string SpeedLJogX { get; set; }
        public string SpeedHJogX { get; set; }
        public string DisStep_JogX { get; set; }
        public string PointABSX { get; set; }
        public string SpABSX { get; set; }
        public string OriginSpeedX { get; set; }
        public string SpStartX { get; set; }
        public string AccX { get; set; }
        public string DecX { get; set; }
        public string PosReadyX { get; set; }
        public string SpReadyX { get; set; }
        public string ABSX { get; set; }
        public string ORGX { get; set; }
        public string JOG__X { get; set; }
        public string JOG_X { get; set; }
        public string RESETX { get; set; }
        #endregion
        #region Axis Y
        public string TimeOutY { get; set; }
        public string DisStep__JogY { get; set; }
        public string SpeedLJogY { get; set; }
        public string SpeedHJogY { get; set; }
        public string DisStep_JogY { get; set; }
        public string PointABSY { get; set; }
        public string SpABSY { get; set; }
        public string OriginSpeedY { get; set; }
        public string SpStartY { get; set; }
        public string AccY { get; set; }
        public string DecY { get; set; }
        public string PosReadyY { get; set; }
        public string SpReadyY { get; set; }
        public string ABSY { get; set; }
        public string ORGY { get; set; }
        public string JOG__Y { get; set; }
        public string JOG_Y { get; set; }
        public string RESETY { get; set; }
        #endregion
        #region Axis Z
        public string TimeOutZ { get; set; }
        public string DisStep__JogZ { get; set; }
        public string SpeedLJogZ { get; set; }
        public string SpeedHJogZ { get; set; }
        public string DisStep_JogZ { get; set; }
        public string PointABSZ { get; set; }
        public string SpABSZ { get; set; }
        public string OriginSpeedZ { get; set; }
        public string SpStartZ { get; set; }
        public string AccZ { get; set; }
        public string DecZ { get; set; }
        public string PosReadyZ { get; set; }
        public string SpReadyZ { get; set; }
        public string ABSZ { get; set; }
        public string ORGZ { get; set; }
        public string JOG__Z { get; set; }
        public string JOG_Z { get; set; }
        public string RESETZ { get; set; }
        #endregion
        #region Axis F
        public string TimeOutF { get; set; }
        public string DisStep__JogF { get; set; }
        public string SpeedLJogF { get; set; }
        public string SpeedHJogF { get; set; }
        public string DisStep_JogF { get; set; }
        public string PointABSF { get; set; }
        public string SpABSF { get; set; }
        public string OriginSpeedF { get; set; }
        public string SpStartF { get; set; }
        public string AccF { get; set; }
        public string DecF { get; set; }
        public string PosReadyF { get; set; }
        public string SpReadyF { get; set; }
        public string ABSF { get; set; }
        public string ORGF { get; set; }
        public string JOG__F { get; set; }
        public string JOG_F { get; set; }
        public string RESETF { get; set; }
        #endregion
        #region Axis RI
        public string DisStep__JogRI { get; set; }
        public string DisStep_JogRI { get; set; }
        public string SpeedLJogRI { get; set; }
        public string SpeedHJogRI { get; set; }
        public string SpStartRI { get; set; }
        public string AccRI { get; set; }
        public string DecRI { get; set; }
        public string ORGRI { get; set; }
        public string JOG__RI { get; set; }
        public string JOG_RI { get; set; }
        public string RESETRI { get; set; }
        #endregion
        #region Axis RO
        public string DisStep__JogRO { get; set; }
        public string DisStep_JogRO { get; set; }
        public string SpeedLJogRO { get; set; }
        public string SpeedHJogRO { get; set; }
        public string SpStartRO { get; set; }
        public string AccRO { get; set; }
        public string DecRO { get; set; }
        public string ORGRO { get; set; }
        public string JOG__RO { get; set; }
        public string JOG_RO { get; set; }
        public string RESETRO { get; set; }
        #endregion
        #region Position Table
        public string PosStartOKX { get; set; }
        public string PosStartOKY { get; set; }
        public string PosEndXOKX { get; set; }
        public string PosEndXOKY { get; set; }
        public string PosEndYOKX { get; set; }
        public string PosEndYOKY { get; set; }
        public string PosStartNGX { get; set; }
        public string PosStartNGY { get; set; }
        public string PosEndXNGX { get; set; }
        public string PosEndXNGY { get; set; }
        public string PosEndYNGX { get; set; }
        public string PosEndYNGY { get; set; }
        public string nXOK_NGmax { get; set; }
        public string nYOK_NGmax { get; set; }
        public string mYNG1 { get; set; }
        public string mYNG2 { get; set; }
        public string mYNG3 { get; set; }
        public string PosStartNG4X { get; set; }
        public string PosStartNG4Y { get; set; }
        public string PosEndXNG4X { get; set; }
        public string PosEndXNG4Y { get; set; }
        public string PosEndYNG4X1_2 { get; set; }
        public string PosEndYNG4Y1_2 { get; set; }
        public string mYNG4max1_2 { get; set; }
        public string PosY1_2StartX { get; set; }
        public string PosY1_2StartY { get; set; }
        #endregion
        #region Socket / Camera / Position Read
        public string PosSKRIX { get; set; }
        public string PosSKRIY { get; set; }
        public string DisRO_RIX { get; set; }
        public string DisRO_RIY { get; set; }
        public string PosrdZ00 { get; set; }
        public string PosrdZI1O0 { get; set; }
        public string PosrdZI1O1 { get; set; }
        public string PosrdZ11 { get; set; }
        public string PosULP0Z { get; set; }
        public string PosLP1Z { get; set; }
        public string PosP2Z { get; set; }
        public string PosULP3Z { get; set; }
        public string PosLP4Z { get; set; }
        public string PosCloseF2 { get; set; }
        public string PosOpenF2 { get; set; }
        public string AngeRO { get; set; }
        public string SpRdP2XY { get; set; }
        public string PosCX { get; set; }
        public string PosCY { get; set; }
        public string SprdZ { get; set; }
        public string RunPosCam { get; set; }
        public string LoadPosCam { get; set; }
        public string SpULP0XY { get; set; }
        public string SpULP0Z { get; set; }
        public string PosrdP0X { get; set; }
        public string PosrdP0Y { get; set; }
        public string SprdP0XY { get; set; }
        public string SpRO { get; set; }
        public string SpLP1XY { get; set; }
        public string SpLP1Z { get; set; }
        public string PosrdP1X { get; set; }
        public string PosrdP1Y { get; set; }
        public string SprdP1XY { get; set; }
        public string SpCP2XY { get; set; }
        public string IdJob { get; set; }
        public string SpRunCRI { get; set; }
        public string PosRdP2X { get; set; }
        public string PosRdP2Y { get; set; }
        public string PosRdP2XY { get; set; }
        public string SpCP2Z { get; set; }
        public string SpULP3XY { get; set; }
        public string SpULP3Z { get; set; }
        public string PosRdP3X { get; set; }
        public string PosRdP3Y { get; set; }
        public string SpRdP3XY { get; set; }
        public string SpLP4XY { get; set; }
        public string SpLP4Z { get; set; }
        public string PosRdP4X { get; set; }
        public string PosRdP4Y { get; set; }
        public string SpRdP4XY { get; set; }
        public string SpCloseF { get; set; }
        public string SpOpenF { get; set; }
        public string RunOpenF2 { get; set; }
        #endregion
        #region Model / Load / Run
        public string RqCallmodel { get; set; }
        public string RqSaveModel { get; set; }
        public string RqSaveModelAs { get; set; }
        public string ModelSaveAs { get; set; }
        public string Skip { get; set; }
        public string LoadPointStart { get; set; }
        public string RunPointStart { get; set; }
        public string LoadPointEndX { get; set; }
        public string RunPointEndX { get; set; }
        public string LoadPointEndY { get; set; }
        public string RunPointEndY { get; set; }
        public string LoadPointStartNG { get; set; }
        public string RunPointStartNG { get; set; }
        public string LoadPointEndXNG { get; set; }
        public string RunPointEndXNG { get; set; }
        public string LoadPointEndYNG { get; set; }
        public string RunPointEndYNG { get; set; }
        public string LoadPointStartNG4 { get; set; }
        public string RunPointStartNG4 { get; set; }
        public string LoadPointEndXNG4 { get; set; }
        public string RunPointEndXNG4 { get; set; }
        public string LoadPointEndYNG4 { get; set; }
        public string RunPointEndYNG4 { get; set; }
        public string LoadPointStart1_2NG4 { get; set; }
        public string RunPointStart1_2NG4 { get; set; }
        public string LoadPointSK { get; set; }
        public string RunPointSK { get; set; }
        public string NameModel { get; set; }
        public string LoadReadyZ00 { get; set; }
        public string RunReadyZ00 { get; set; }
        public string LoadReadyZ10 { get; set; }
        public string RunReadyZ10 { get; set; }
        public string LoadReadyZ01 { get; set; }
        public string RunReadyZ01 { get; set; }
        public string LoadReadyZ11 { get; set; }
        public string RunReadyZ11 { get; set; }
        public string LoadULP0Z { get; set; }
        public string RunULP0Z { get; set; }
        public string LoadLP1Z { get; set; }
        public string RunLP1Z { get; set; }
        public string LoadCP2Z { get; set; }
        public string RunCP2Z { get; set; }
        public string LoadULP3Z { get; set; }
        public string RunULP3Z { get; set; }
        public string LoadLP4Z { get; set; }
        public string RunLP4Z { get; set; }
        public string LoadCloseF2 { get; set; }
        public string RunCloseF2 { get; set; }
        public string LoadOpenF2 { get; set; }
        public string ReCheck { get; set; }
        #endregion
        #region NG Result
        public string RsNG101 { get; set; }
        public string RsNG102 { get; set; }
        public string RsNG103 { get; set; }
        public string RsNG104 { get; set; }
        public string RsNG105 { get; set; }
        public string RsNG106 { get; set; }
        public string RsNG107 { get; set; }
        public string RsNG108 { get; set; }
        public string RsNG109 { get; set; }
        public string RsNG110 { get; set; }
        public string RsNG111 { get; set; }
        public string RsNG112 { get; set; }
        public string RsNG113 { get; set; }
        public string RsNG114 { get; set; }
        public string RsNG115 { get; set; }
        public string RsNG116 { get; set; }
        public string RsNG117 { get; set; }
        public string RsNG118 { get; set; }
        public string RsNG119 { get; set; }
        public string RsNG120 { get; set; }
        public string RsNG201 { get; set; }
        public string RsNG202 { get; set; }
        public string RsNG203 { get; set; }
        public string RsNG204 { get; set; }
        public string RsNG205 { get; set; }
        public string RsNG206 { get; set; }
        public string RsNG207 { get; set; }
        public string RsNG208 { get; set; }
        public string RsNG209 { get; set; }
        public string RsNG210 { get; set; }
        public string RsNG211 { get; set; }
        public string RsNG212 { get; set; }
        public string RsNG213 { get; set; }
        public string RsNG214 { get; set; }
        public string RsNG215 { get; set; }
        public string RsNG216 { get; set; }
        public string RsNG217 { get; set; }
        public string RsNG218 { get; set; }
        public string RsNG219 { get; set; }
        public string RsNG220 { get; set; }
        public string RsNG301 { get; set; }
        public string RsNG302 { get; set; }
        public string RsNG303 { get; set; }
        public string RsNG304 { get; set; }
        public string RsNG305 { get; set; }
        public string RsNG306 { get; set; }
        public string RsNG307 { get; set; }
        public string RsNG308 { get; set; }
        public string RsNG309 { get; set; }
        public string RsNG310 { get; set; }
        public string RsNG311 { get; set; }
        public string RsNG312 { get; set; }
        public string RsNG313 { get; set; }
        public string RsNG314 { get; set; }
        public string RsNG315 { get; set; }
        public string RsNG316 { get; set; }
        public string RsNG317 { get; set; }
        public string RsNG318 { get; set; }
        public string RsNG319 { get; set; }
        public string RsNG320 { get; set; }
        public string RsNG401 { get; set; }
        public string RsNG402 { get; set; }
        public string RsNG403 { get; set; }
        public string RsNG404 { get; set; }
        public string RsNG405 { get; set; }
        public string RsNG406 { get; set; }
        public string RsNG407 { get; set; }
        public string RsNG408 { get; set; }
        public string RsNG409 { get; set; }
        public string RsNG410 { get; set; }
        public string RsNG411 { get; set; }
        public string RsNG412 { get; set; }
        public string RsNG413 { get; set; }
        public string RsNG414 { get; set; }
        public string RsNG415 { get; set; }
        public string RsNG416 { get; set; }
        public string RsNG417 { get; set; }
        public string RsNG418 { get; set; }
        public string RsNG419 { get; set; }
        public string RsNG420 { get; set; }
        #endregion
        #region Cur Pos
        public string CurPosX { get; set; }
        public string CurPosY { get; set; }
        public string CurPosZ { get; set; }
        public string CurPosF { get; set; }
        public string CurPosRI { get; set; }
        public string CurPosRO { get; set; }
        #endregion
        public string PosrdZI0O1 { get; set; }
        #region CVCameraAxis
        public string CVCameraAxisX { get; set; }
        public string CVCameraAxisY { get; set; }
        public string CVCameraAxisZ { get; set; }
        #endregion
        #region CVMaterAxis
        public string CVMaterAxisX { get; set; }
        public string CVMaterAxisY { get; set; }
        public string CVMaterAxisZ { get; set; }
        #endregion
        public string LoadMasterCam { get; set; }
        public string Retrig { get; set; }
        public string RqVcam { get; set; }
        #region ReCheck
        public string ReCheck01 { get; set; }
        public string ReCheck02 { get; set; }
        public string ReCheck03 { get; set; }
        public string ReCheck04 { get; set; }
        public string ReCheck05 { get; set; }
        public string ReCheck06 { get; set; }
        public string ReCheck07 { get; set; }
        public string ReCheck08 { get; set; }
        public string ReCheck09 { get; set; }
        public string ReCheck10 { get; set; }
        //---
        public string NumberReCheck01 { get; set; }
        public string NumberReCheck02 { get; set; }
        public string NumberReCheck03 { get; set; }
        public string NumberReCheck04 { get; set; }
        public string NumberReCheck05 { get; set; }
        public string NumberReCheck06 { get; set; }
        public string NumberReCheck07 { get; set; }
        public string NumberReCheck08 { get; set; }
        public string NumberReCheck09 { get; set; }
        public string NumberReCheck10 { get; set; }
        #endregion
        public string TimeOutTester { get; set; }
        public string TimeOutData { get; set; }
        public string RqSensorDoor { get; set; }
        public string RqSensorTray { get; set; }
        public string DryRun { get; set; }
        public string OFFX { get; set; }
        public string OFFY { get; set; }
        public string OFFZ { get; set; }
        public string OFFF { get; set; }
        public string OFFRI { get; set; }
        public string OFFRO { get; set; }
        public string Hight { get; set; }
        public string Mid { get; set; }
        public string Low { get; set; }
        public string Micro { get; set; }
        public string Chart { get; set; }
    }
    public class DataforUnload
    {
        public string nXULoadNGNow { get; set; }
        public string mYULoadNGNow { get; set; }
        public string RsOUTlca { get; set; }
        public string RsClassifylca { get; set; }
        public string TakePointNG { get; set; }
        public string ResetTrayNG { get; set; }
        public string nXloadnow { get; set; }
        public string nYloadnow { get; set; }
    }
    public class DataforloadImei
    {
        public string nXULoadNGNow { get; set; }
        public string mYULoadNGNow { get; set; }
        public string TakePointNG { get; set; }
        public string ResetTrayNG { get; set; }
    }
    public class Dataforload
    {
        public string nXLoadOK { get; set; }
        public string mYLoadOK { get; set; }
        public string TakePointOK { get; set; }
        public string ResetTrayOK { get; set; }
    }
    public class DataforNG4
    {
        public string nXULoadNG4 { get; set; }
        public string mXULoadNG4 { get; set; }
        public string RsOUTlca { get; set; }
        public string RsClassifylca { get; set; }
        public string TakePointNG4 { get; set; }
        public string ResetTrayNG4 { get; set; }
        public string CurPosZ { get; set; }
        public string nXloadnow { get; set; }
        public string nYloadnow { get; set; }
    }
    public class DataforInputResults
    {
        public string nXULoadNGNow { get; set; }
        public string mYULoadNGNow { get; set; }
        public string RsOUTlca { get; set; }
        public string RsClassifylca { get; set; }
        public string nXLoadOK { get; set; }
        public string mXULoadOK { get; set; }
        public string nXULoadNG4 { get; set; }
        public string mXULoadNG4 { get; set; }
        public string CurPosX { get; set; }
        public string CurPosY { get; set; }
        public string CurPosRI { get; set; }
        public string CurPosRo { get; set; }
        public string CurPosZ { get; set; }
        public string CurPosF { get; set; }
        public string NberPrdIn { get; set; }
        public string NberPrdOK { get; set; }
        public string NberPrdNG { get; set; }
        public string NberPrdOut { get; set; }
        public string percentOK { get; set; }
        public string percentNG { get; set; }
        public string TimeS { get; set; }
        public string TimeM { get; set; }
        public string TimeH { get; set; }
        public string CircleTimeOK { get; set; }
        public string CircleTimeNG { get; set; }
        public string Condicamera1 { get; set; }
        public string Condicamera2 { get; set; }
        public string VFix2 { get; set; }
        public string VCHART { get; set; }
        public string VChckSk2 { get; set; }
        public string EMS2 { get; set; }
        public string ResetWork { get; set; }
        public string SetCompWork { get; set; }
        public string SetEmptyWork { get; set; }
        public string ModifyWork { get; set; }
        public string ResetCompelete { get; set; }
        public string SetCompCompelete { get; set; }
        public string SetEmptyCompelete { get; set; }
        public string ModifyCompelete { get; set; }
        public string ModifyWork2 { get; set; }
        public string ModifyCompelete2 { get; set; }
        public string ChangeModeTrayInput { get; set; }
        public string Tray1InputSignal { get; set; }
        public string Tray2InputSignal { get; set; }
        //
        public string Skip { get; set; }
        public string Retri { get; set; }
        public string Auto { get; set; }
        public string EndAuto { get; set; }
        public string ResetData { get; set; }
        public string ResetNG4 { get; set; }
        public string ResetDataTable { get; set; }
        public string ResetTrayNG { get; set; }
        public string DryRun { get; set; }
        public string ReStartNoData { get; set; }
        public string Loto { get; set; }
        public string Restart { get; set; }
    }
}