using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using project.Models;

namespace project
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private bool isFavorite = false;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        

        private async void BtnReqRealtime_Click(object sender, RoutedEventArgs e)
        {
            string openApiUri = "https://apis.data.go.kr/5090000/healthCentersAndClinicsService/getHealthCentersAndClinics?serviceKey=Ns3hWuwudEELiSsTpvt3f4vsZrD%2Fldxvl265Zma2pRwWUBLYxIEqkaTF%2BkhzLvmUmhjX1c5s%2BckCnyaJYWzi8Q%3D%3D&pageNo=1&numOfRows=10&NM=%ED%92%8D%EA%B8%B0%EC%9D%8D%EB%B3%B4%EA%B1%B4%EC%A7%80%EC%86%8C";
            string result = string.Empty;

            // WebRequest, WebResponse 객체
            WebRequest req = null;
            WebResponse res = null;
            StreamReader reader = null;

            try
            {
                req = WebRequest.Create(openApiUri);
                res = await req.GetResponseAsync();
                reader = new StreamReader(res.GetResponseStream());
                result = await reader.ReadToEndAsync();

                //await this.ShowMessageAsync("결과", result);
                Debug.WriteLine(result);
            }
            catch (Exception ex)
            {

                await this.ShowMessageAsync("오류", $"OpenAPI 조회오류 {ex.Message}");
            }

            var jsonResult = JObject.Parse(result);
            var resultCode = Convert.ToString(jsonResult["response"]["header"]["resultCode"]);

            if (resultCode == "00")
            {
                var data = jsonResult["response"]["body"]["items"]["item"];
                var jsonArray = data as JArray;

                var HospitalCenters = new List<HospitalCenter>();
                foreach (var item in jsonArray)
                {
                    HospitalCenters.Add(new HospitalCenter()
                    {
                       Id = 0,
                        NM = Convert.ToString(item["NM"]),
                        LC = Convert.ToString(item["LC"]),
                        TELNO = Convert.ToString(item["TELNO"]),
                        OPER_BGNG_TM = Convert.ToString(item["OPER_BGNG_TM"]),
                        OPER_END_TM = Convert.ToString(item["OPER_END_TM"]),
                        CLNIC_SCOPE = Convert.ToString(item["CLNIC_SCOPE"])
                    });
                }
                this.DataContext = HospitalCenters;
                StsResult.Content = $"OpenAPI{HospitalCenters.Count}건 조회완료!";
            }

        }

        

        private void GrdResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var curlItem = GrdResult.SelectedItem as HospitalCenter;

            var mapWindow = new MapWindow(curlItem.LC);
            mapWindow.Owner = this;
            mapWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            mapWindow.ShowDialog();

        }

        //private async void BtnSaveData_Click(object sender, RoutedEventArgs e)
        //{
        //    if (GrdResult.Items.Count == 0)
        //    {
        //        await this.ShowMessageAsync("저장오류", "실시간 조회후 저장하십시오.");
        //        return;
        //    }

        //    try
        //    {
        //        using (SqlConnection conn = new SqlConnection(Helpers.Common.CONNSTRING))
        //        {
        //            conn.Open();

        //            var insRes = 0;
        //            foreach (HospitalCenter item in GrdResult.Items)
        //            {
        //                SqlCommand cmd = new SqlCommand(Models.HospitalCenter.INSERT_QUERY, conn);
        //                cmd.Parameters.AddWithValue("@NM", item.NM);
        //                cmd.Parameters.AddWithValue("@LC", item.LC);
        //                cmd.Parameters.AddWithValue("@TELNO", item.TELNO);
        //                cmd.Parameters.AddWithValue("@OPER_BGNG_TM", item.OPER_BGNG_TM);
        //                cmd.Parameters.AddWithValue("@OPER_END_TM", item.OPER_END_TM);
        //                cmd.Parameters.AddWithValue("@CLNIC_SCOPE", item.CLNIC_SCOPE);

        //                insRes += cmd.ExecuteNonQuery();
        //            }

        //            if (insRes > 0)
        //            {
        //                await this.ShowMessageAsync("저장", "DB저장성공!");
        //                StsResult.Content = $"DB저장 {insRes}건 성공!";
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await this.ShowMessageAsync("저장오류", $"저장오류 {ex.Message}");
        //    }
        //}

        private async void BtnAddFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (GrdResult.SelectedItems.Count == 0)
            {
                await this.ShowMessageAsync("즐겨찾기", "추가할 보건소를 선택하세요(복수선택가능).");
                return;
            }
            if (isFavorite == true)
            {
                await this.ShowMessageAsync("즐겨찾기", "이미 즐겨찾기한 보건소입니다.");
                return;
            }

            var addHosipitalCenters = new List<HospitalCenter>();
            foreach (HospitalCenter item in GrdResult.SelectedItems)
            {
                addHosipitalCenters.Add(item);
            }
            Debug.WriteLine(addHosipitalCenters.Count);

            try
            {
                var insRes = 0;
                using (SqlConnection conn = new SqlConnection(Helpers.Common.CONNSTRING))
                {
                    conn.Open();

                    foreach (HospitalCenter item in addHosipitalCenters)
                    {
                        SqlCommand chkCmd = new SqlCommand(HospitalCenter.CHECK_QUERY, conn);
                        chkCmd.Parameters.AddWithValue("@Id", item.Id);
                        var cnt = Convert.ToInt32(chkCmd.ExecuteScalar());

                        if (cnt > 0) continue;

                        SqlCommand cmd = new SqlCommand(Models.HospitalCenter.INSERT_QUERY, conn);
                        cmd.Parameters.AddWithValue("@NM", item.NM);
                        cmd.Parameters.AddWithValue("@LC", item.LC);
                        cmd.Parameters.AddWithValue("@TELNO", item.TELNO);
                        cmd.Parameters.AddWithValue("@OPER_BGNG_TM", item.OPER_BGNG_TM);
                        cmd.Parameters.AddWithValue("@OPER_END_TM", item.OPER_END_TM);
                        cmd.Parameters.AddWithValue("@CLNIC_SCOPE", item.CLNIC_SCOPE);

                        insRes += cmd.ExecuteNonQuery();
                    }
                }
                if (insRes == addHosipitalCenters.Count)
                {
                    await this.ShowMessageAsync("즐겨찾기", $"즐겨찾기 {insRes}건저장성공!");
                }
                else
                {
                    await this.ShowMessageAsync("즐겨찾기", $"즐겨찾기 {addHosipitalCenters.Count}건 중 {insRes}건 저장성공!");
                }

            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("오류", $"즐겨찾기 오류 {ex.Message}");
            }
            
            BtnViewFavorite_Click(sender, e);
        }

        private async void BtnDelFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (isFavorite == false)
            {
                await this.ShowMessageAsync("삭제", "즐겨찾기한 보건소가 아닙니다.");
                return;
            }
            if (GrdResult.SelectedItems.Count == 0)
            {
                await this.ShowMessageAsync("삭제", "삭제할 보건소를 선택하세요.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(Helpers.Common.CONNSTRING))
                {
                    conn.Open();

                    var delRes = 0;

                    foreach (HospitalCenter item in GrdResult.SelectedItems)
                    {
                        SqlCommand cmd = new SqlCommand(Models.HospitalCenter.DELETE_QUERY, conn);
                        cmd.Parameters.AddWithValue("@Id", item.Id);

                        delRes += cmd.ExecuteNonQuery();
                    }

                    if (delRes == GrdResult.SelectedItems.Count)
                    {
                        await this.ShowMessageAsync("삭제", $"즐겨찾기 {delRes}건 삭제");
                    }
                    else
                    {
                        await this.ShowMessageAsync("삭제", $"즐겨찾기 {GrdResult.SelectedItems.Count}건중 {delRes} 건 삭제");
                    }
                }

            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("오류", $"즐겨찾기 조회오류 {ex.Message}");
            }
            BtnViewFavorite_Click(sender, e);
        }

        private async void BtnViewFavorite_Click(object sender, RoutedEventArgs e)
        {
            this.DataContext = null;
            List<HospitalCenter> favHoispitalCenters = new List<HospitalCenter>();

            try
            {
                using (SqlConnection conn = new SqlConnection(Helpers.Common.CONNSTRING))
                {
                    conn.Open();

                    var cmd = new SqlCommand(Models.HospitalCenter.SELECT_QUERY, conn);
                    var adapter = new SqlDataAdapter(cmd);
                    var dSet = new DataSet();
                    adapter.Fill(dSet, "HospitalCenter");

                    foreach (DataRow row in dSet.Tables["HospitalCenter"].Rows)
                    {
                        var hospitalitem = new HospitalCenter()
                        {
                            Id = Convert.ToInt32(row["Id"]),
                            NM = Convert.ToString(row["NM"]),
                            LC = Convert.ToString(row["LC"]),
                            CLNIC_SCOPE = Convert.ToString(row["CLNIC_SCOPE"]),
                            OPER_BGNG_TM = Convert.ToString(row["OPER_BGNG_TM"]),
                            OPER_END_TM = Convert.ToString(row["OPER_END_TM"]),
                            TELNO = Convert.ToString(row["TELNO"])
                        };

                        favHoispitalCenters.Add(hospitalitem);
                    }
                    this.DataContext = favHoispitalCenters;
                    isFavorite = true;
                    StsResult.Content = $"즐겨찾기 {favHoispitalCenters.Count}건 조회완료";

                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("오류", $"즐겨찾기 조회오류 {ex.Message}");
            }
        }
    }
}