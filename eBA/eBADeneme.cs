using eBAControls;
using eBAFlowScrAdp;

using eBAPI.DocumentManagement;
using eBAPI.Connection;
using eBAPI.Workflow;
using eBADB;
using eBAFormData;
using eBADBHelper;
using eBALogAPIHelper.Helper;
using System.Data.SqlClient;
using System.Data;
using System.Web;
using eBAPDFExport;
using eBALibrary;
using eBAContext;

using ebanet;
using eBAIntegrationAPI;
using eBAFlowScrAdp.Objects;
using eBAPI.DocumentManagement.Security;
using eBAControls.eBABaseForm;
using TCMBCurrencies;
using eBAMailAPI;
using Microsoft.VisualBasic;
using System.Data.Common;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System;


Delegation delegation = new Delegation();

DateTime dtStart = new DateTime(2022, 08, 11);
DateTime dtEnd = new DateTime(2022, 08, 10);
if (dtStart > dtEnd)
{
    Console.WriteLine("b");
}



TimeSpan ts = dtStart - dtEnd;

DateTime date = dtStart;

int i = 1;
if (dtEnd.DayOfWeek == DayOfWeek.Friday)
{
    i += 1;
}
do
{
    if (!IsWeekEnd(date) && !IsHoliday(date))
    {
        i++;
    }

    date = date.AddDays(1);

} while (dtEnd != date);

Console.WriteLine($"{i}-{ts.TotalDays}");

Console.ReadLine();


override protected void internalOnPageLoad(Object sender, EventArgs e)
{
    var dt = new eBADetailsGrid();
    base.internalOnPageLoad(sender, e);

    foreach (var c in dt.GetControls("txtSayi1"))
    {
        c.Visible = false;
    }

    foreach (var c in eBADetailsGrid.Columns[0].Visible)
    {
        if (c.Name == "txtSayi1")
        {
            c.Control.Visible = false;
        }
    }
}

void dateKontrol()
{


    string a = "";
    if (a == "Dogum İzni")
    {
        DateTime bas = new DateTime();
        DateTime son = new DateTime();
        TimeSpan ts = (son - bas);
        double d = ts.TotalDays;
        if (d >= 102)
        {
            ShowMessageBox("Doğum öncesi ve sonrası olmak üzere toplam 16 hafta izin kullanılabilmektedir");
        }

    }


    //saat kontrol

    if (a == "Saatlik İzin")
    {
        DateTime dtBas = new DateTime();
        DateTime dtSon = new DateTime();
        TimeSpan saatBas = dtBas.TimeOfDay;
        TimeSpan saatSon = dtBas.TimeOfDay;

        TimeSpan time = (saatSon - saatBas);
        double dTime = time.TotalMinutes;
        if (dTime > 90)
        {
            ShowMessageBox("Saatlik İzniniz Azami 1.5 saattir");
        }
    }


}

void onclick()
{

    eBAConnection con = CreateServerConnection();
    con.Open();

    FileSystem fs = con.FileSystem;

    DMFile form = fs.GetFile("workflow/SozlesmeYonetimi/frmSozlesmeTanim/" + id.ToString() + ".wfd"); //Document1.Path  
    foreach (DMFileContent dmc in form.GetAttachments(""))
    {
        string destPath = "files/Temp/" + dmc.ContentName;
        DMFile att = fs.CreateFile(destPath);
        att.UploadContentFromByteArray(form.DownloadAttachmentContentBytes("default", dmc.ContentName));  //dmc.ContentName

    }


    string name = "";
    foreach (DataRow dr in Table1.Data.Rows)
    {
        if (dr["CHECKED"].ToString() == "1")
        {


            //GetShareLink(DökümanPathı, Paylaşım Tipi (External(herkses açık),Internal(Program kullanıcıları)),Paylaşım başlangıç tarihi,Paylaşım Bitiş Tarihi( null sonsuza kadar), Tıklama sayısı (0 sonsuz adet))
            string ShareLink = fs.GetShareLink(name, eBAPI.DocumentManagement.ShareLinkType.External, DateTime.Now, null, 0);
            //Etiket2.Text = "<a href='" + ShareLink + "'>Link</a>";
            dr["Link"] = "<a href='" + ShareLink + "'>Link</a>";
        }
    }

    con.Close();

}

void update()
{
    //Table Senkronize

    eBAForm senkEden = new eBAForm(formId);
    eBAForm senkEdilen = new eBAForm(Convert.ToInt32(id.ToString()));


    FormDetailsGrid AnaDg = senkEden.DetailsGrids["dtlUrunler"];
    FormDetailsGrid altDg = senkEdilen.DetailsGrids["dtl_altakis"];

    foreach (FormDetailsGridRow anaDgr in AnaDg.Rows)
    {
        FormDetailsGridRow alt_row = altDg.Rows.Add();

        alt_row["txtUrun"].AsString = anaDgr["txtUrun"].ToString();
        alt_row["txtUrunAdet"].AsString = anaDgr["txtUrunAdet"].ToString();
        //alt_row["txt_TeslimTarihi"].AsString = anaDgr["txt_TeslimTarihi"].ToString();
        //alt_row["txt_Aciklama"].AsString = anaDgr["txt_Aciklama"].ToString();
    }

    senkEdilen.Update();
}

override protected void internalOnPageLoad(Object sender, EventArgs e)
{
    base.internalOnPageLoad(sender, e);

}

static bool IsWeekEnd(DateTime date)
{
    return date.DayOfWeek == DayOfWeek.Saturday
        || date.DayOfWeek == DayOfWeek.Sunday;
}

static bool IsHoliday(HashSet<DateTime> holidays, DateTime date)
{

    return holidays.Contains(date);
}

HashSet<DateTime> GetHolidays()
{
    HashSet<DateTime> Holidays = new HashSet<DateTime>();
    eBAConnection con = CreateServerConnection();

    eBAForm prmForm = new eBAForm(64971);
    FormDetails dtVacationDays = prmForm.Details["DTY_VacationDays"];

    foreach (var Row in dtVacationDays.Rows)     //Sırayla satırların formunda geziyoruz
    {
        eBAForm ModalForm = Row.Form;

        /* WorkflowManager mng = con.WorkflowManager;
        WorkflowDocument doc = mng.CreateDocument("IzinTalepSureci", "TatilGunleriTanimlamaMDL"); //Detayların Bağlı Olduğu Form  */

        DateTime dtStart = ModalForm.Fields["txt_StartingDate"].AsDateTime;
        DateTime dtEnd = ModalForm.Fields["txt_EndDate"].AsDateTime;

        for (DateTime dt = dtStart; dt <= dtEnd; dt.AddDays(1))
        {
            Holidays.Add(dt);
        }
    }

    return Holidays;
}
//Organization.GetUser(LogonUser).Department.ToString();

#region eBA

eBALogAPI logApi = new eBALogAPI("OK_SINAV_", "TEST");     //Proje İsmi, Instance İsmi     
void WriteLog(string caption, string description, Exception ex = null)
{
    logApi.AddLogAsync(logText: caption, logDetailsText: description, logType: ex == null ? eBALogType.None : eBALogType.Error, userId: "", exception: ex);
}

void HolidayControl()
{
    try
    {
        if (dtEnd.DayOfWeek == DayOfWeek.Friday)
            i += 1;

        do
        {
            if (!IsWeekEnd(date) && !IsHoliday(GetHolidays(), date))   // !IsHoliday(Holidays,date)
                i++;

            date = date.AddDays(1);
        } while (dtEnd != date);

        /*txt_Total.Text = i.ToString();

        if (dtEnd.DayOfWeek == DayOfWeek.Saturday)
            dt_StartWork.Value = dtEnd.AddDays(2);
        else if (dtEnd.DayOfWeek == DayOfWeek.Friday)
            dt_StartWork.Value = dtEnd.AddDays(3);
        else
        {
            dt_StartWork.Value = dtEnd.AddDays(1);
            if (dt_StartWork.Value == dtZafer)
                dt_StartWork.Value = dtEnd.AddDays(2);
        }*/
    }
    catch
    {

    }

}
void ShowModForm()
{

    //BaseForm.ShowModalForm(sender: Button1, project: "CRF", form: "CustomerRegistrationForm", view: "default", create: true, documentId: id , readOnly: false);
}


void TCMBCurreinces()
{
    double curUsd = CurrenciesExchange.CalculateTodaysExchange(2000, CurrencyCode.USD, CurrencyCode.TRY);
    double curEur = CurrenciesExchange.CalculateTodaysExchange(2000, CurrencyCode.EUR, CurrencyCode.TRY);
    double curOldUsd = CurrenciesExchange.CalculateHistoricalExchange(2000, CurrencyCode.USD, CurrencyCode.TRY, new System.DateTime(1, 1, 1));
    double curOldEur = CurrenciesExchange.CalculateHistoricalExchange(2000, CurrencyCode.EUR, CurrencyCode.TRY, new System.DateTime(1, 1, 1));

}
void moveFile()
{

    //FileSystem fs = new FileSystem();

    //fs.MoveFile(sourcePath: , destinationPath:);
}

void CopyAttachments()
{



}



void ProfilForm()
{
    Path.GetFileName(CreateDocumentPath);
}

void DetaylarEkleme()
{
    eBAConnection con = CreateServerConnection();
    con.Open();
    try
    {
        WorkflowDocument detayForm = con.WorkflowManager.CreateDocument("D20_1", "dtlModal"); //İlgili formdaki modal formu oluşturuyoruz
        eBAForm frm = new eBAForm(detayForm.DocumentId); //Oluşturduğumu modal forma ulaşıyoruz.
        DataRow row = Details1.CreateRow();
        frm.Fields["Text1"].AsString = alinacakDok.Fields["Text1"].AsString;
        frm.Fields["Text2"].AsString = alinacakDok.Fields["Text2"].AsString;

        //Tablo satırında verilerin gözükmesi için.   
        row["DOCUMENTID"] = detayForm.DocumentId;
        row["Text1"] = alinacakDok.Fields["Text1"].AsString;
        row["Text2"] = alinacakDok.Fields["Text2"].AsString;
        frm.Update();
        Details1.InsertRow(row);
        RefreshDetails(Details1);
    }
    catch (Exception ex)
    {
        WriteLog("D20_1 Projesi" + " -Süreç No: " + id, "Basılan Buton: btnLog\n Tarih: " + +"\n İşlemi Yapan: " + LogonUser, null);
        //WriteLog("\nD20_1 Projesi"+"\nSüreç No: "+id, "\nBasılan Buton: btnLog\nTarih: "+DateTime.Now+"\nİşlemi Yapan: "+LogonUser+"\nView: "+CurrentView , null);    
        throw new Exception(ex.Message);
    }
    finally
    {
        con.Close();
    }
}

void NetVersion()
{
    const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

    using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
    {
        if (ndpKey != null && ndpKey.GetValue("Release") != null)
        {
            Console.WriteLine($".NET Framework Version: {CheckFor45PlusVersion((int)ndpKey.GetValue("Release"))}");
        }
        else
        {
            Console.WriteLine(".NET Framework Version 4.5 or later is t detected.");
        }
    }

    // Checking the version using >= enables forward compatibility.
    string CheckFor45PlusVersion(int releaseKey)
    {
        if (releaseKey >= 528040)
            return "4.8 or later";
        if (releaseKey >= 461808)
            return "4.7.2";
        if (releaseKey >= 461308)
            return "4.7.1";
        if (releaseKey >= 460798)
            return "4.7";
        if (releaseKey >= 394802)
            return "4.6.2";
        if (releaseKey >= 394254)
            return "4.6.1";
        if (releaseKey >= 393295)
            return "4.6";
        if (releaseKey >= 379893)
            return "4.5.2";
        if (releaseKey >= 378675)
            return "4.5.1";
        if (releaseKey >= 378389)
            return "4.5";
        // This code should never execute. A non-null release key should mean
        // that 4.5 or later is installed.
        return "No 4.5 or later version detected";
    }
}

void KEPVeriAl_Execute()
{
    //KEp Hatasını Çözmek için kullanılıyor.

    #region KEP

    if (vrbHataDeneme.Value == "HataDeneme")
    {
        EVRAKKAYIT2.ClearGroup();
        EVRAKKAYIT2.AddConstantUser("admin");
    }

    eBAForm formData = new eBAForm(Metadata.ProfileId);
    using (eBAConnection con = CreateServerConnection())
    {
        con.Open();
        string filename = "";
        string FormPath = "workflow/GBK/UV/" + Metadata.ProfileId.ToString() + ".wfd";
        FileSystem fs = con.FileSystem;
        DMFile file = fs.GetFile(varEMLPath.Value);
        int pId = 0;

        if (file.ObjectProperties.Profile.HasValue)
        {
            pId = file.ObjectProperties.Profile.Value;
            eBAForm mail = new eBAForm(pId);
            formData.Fields["BGTARIH"].AsDateTime = mail.Fields["TXTDATE"].AsDateTime;
        }
        DMCategoryContentCollection col = file.GetAttachments("EMLAttachments");
        if (file.GetAttachments("EMLAttachments").Where(a => a.Extension == "eyp").Count() > 0)
        {
            for (int i = 0; i < col.Count; i++)
            {
                if (col[i].Extension == "eyp")
                {
                    DMFileContent cont = col[i];
                    Stream st = file.CreateAttachmentContentDownloadStream("EMLAttachments", cont.ContentName);
                    st.Seek(0, SeekOrigin.Begin);
                    int? pver = Cbddo.eYazisma.Tipler.Araclar.PaketVersiyonuGetir(st);
                    if (pver == 1)
                    {
                        Paket p = Paket.Ac(st, PaketModu.Ac);
                        string BelgeNo = "";
                        BelgeNo = p.Ustveri.BelgeNoAl().ToString();
                        formData.Fields["RUID"].AsString = p.Ustveri.BelgeIdAl().ToString();
                        formData.Fields["KONU"].AsString = p.Ustveri.KonuAl().Value;
                        formData.Fields["TARIH"].AsDateTime = p.Ustveri.TarihAl();
                        formData.Fields["BELGENO"].AsString = p.Ustveri.BelgeNoAl().ToString();
                        string dosyaKodu = string.Empty;
                        if (!p.Ustveri.BelgeNoAl().ToString().Contains("["))
                        {
                            if (p.Ustveri.BelgeNoAl().ToString().Contains("-"))
                            {
                                string[] dizi = p.Ustveri.BelgeNoAl().ToString().Split('-');
                                if (dizi.Length > 1)
                                {
                                    dosyaKodu = p.Ustveri.BelgeNoAl().ToString().Split('-')[1];
                                    string SDPPath = GetSdp(dosyaKodu);
                                    if (!string.IsNullOrEmpty(SDPPath))
                                    {
                                        formData.Fields["DOSYAYOLULISTE"].AsString = SDPPath;
                                        formData.Fields["DOSYAYOLULISTE_TEXT"].AsString = SDPPath;
                                    }
                                }
                            }
                        }
                        formData.Fields["GUVKOD"].AsString = p.Ustveri.GuvenlikKoduAl().ToString();
                        formData.Fields["GLSYOLU"].AsString = "KEP";
                        formData.Fields["GLSYOLU_TEXT"].AsString = "KEP";
                        filename = p.Ustveri.BelgeIdAl().ToString() + ".pdf";

                        if (p.Ustveri.OlusturanAl().Item.ToString() == "Dpt.eYazisma.Xsd.CT_KurumKurulus")
                        {
                            formData.Fields["RDLDGT"].AsString = "KK";
                            formData.Fields["RDLDGT_TEXT"].AsString = "Kurum";
                            formData.Fields["OLUSTURANKURUM"].AsString = ((Dpt.eYazisma.Xsd.CT_KurumKurulus)(p.Ustveri.OlusturanAl().Item)).KKK;
                            formData.Fields["OLUSTURANKURUM_TEXT"].AsString = ((Dpt.eYazisma.Xsd.CT_KurumKurulus)(p.Ustveri.OlusturanAl().Item)).Adi.Value;
                            string hiyerarsi = getHiyerarsi(((Dpt.eYazisma.Xsd.CT_KurumKurulus)(p.Ustveri.OlusturanAl().Item)).KKK, ((Dpt.eYazisma.Xsd.CT_KurumKurulus)(p.Ustveri.OlusturanAl().Item)).Adi.Value);
                            formData.Fields["TXTKURUM"].AsString = ((Dpt.eYazisma.Xsd.CT_KurumKurulus)(p.Ustveri.OlusturanAl().Item)).Adi.Value;

                            if (!string.IsNullOrEmpty(hiyerarsi))
                            {
                                formData.Fields["TXTKURUM"].AsString = hiyerarsi;
                            }
                        }
                        if (p.Ustveri.OlusturanAl().Item.ToString() == "Dpt.eYazisma.Xsd.CT_TuzelSahis")
                        {
                            formData.Fields["RDLDGT"].AsString = "TS";
                            formData.Fields["RDLDGT_TEXT"].AsString = "Tüzel Şahıs";
                            formData.Fields["TZLSHS"].AsString = ((Dpt.eYazisma.Xsd.CT_TuzelSahis)(p.Ustveri.OlusturanAl().Item)).Id.Value;
                            formData.Fields["TZLSHSTEXT"].AsString = ((Dpt.eYazisma.Xsd.CT_TuzelSahis)(p.Ustveri.OlusturanAl().Item)).Adi.Value;
                            formData.Fields["TXTKURUM"].AsString = ((Dpt.eYazisma.Xsd.CT_TuzelSahis)(p.Ustveri.OlusturanAl().Item)).Adi.Value;
                        }
                        if (p.Ustveri.OlusturanAl().Item.ToString() == "Dpt.eYazisma.Xsd.CT_GercekSahis")
                        {
                            formData.Fields["RDLDGT"].AsString = "KO";
                            formData.Fields["RDLDGT_TEXT"].AsString = "Kişi";
                            string ilkAd = ((Dpt.eYazisma.Xsd.CT_GercekSahis)(p.Ustveri.OlusturanAl().Item)).Kisi.IlkAdi.Value;
                            string ikinciAd = "";//((Dpt.eYazisma.Xsd.CT_GercekSahis)(p.Ustveri.OlusturanAl().Item)).Kisi.IkinciAdi.Value;
                            string soyad = ((Dpt.eYazisma.Xsd.CT_GercekSahis)(p.Ustveri.OlusturanAl().Item)).Kisi.Soyadi.Value;
                            string fullName = string.Concat(ilkAd, " ", ikinciAd, " ", soyad);
                            formData.Fields["KISISEC"].AsString = fullName;
                            formData.Fields["KISISEC_TEXT"].AsString = fullName;
                            formData.Fields["TXTKURUM"].AsString = fullName;
                        }
                        formData.Update();
                        System.IO.Stream ustYazi = p.UstYaziAl();
                        if (!con.FileSystem.HasFolder("SDP/Kep"))
                        {
                            con.FileSystem.CreateFolder("SDP/Kep");
                        }
                        DMFile form = fs.GetFile(Metadata.Path);
                        string filePath = "SDP/Kep";
                        if (!string.IsNullOrEmpty(formData.Fields["DOSYAYOLULISTE"].AsString))
                        {
                            filePath = formData.Fields["DOSYAYOLULISTE"].AsString;
                        }
                        DMFile newFile = con.FileSystem.CreateFile(filePath + "/" + eBABYSHelper.BYSHelper.UIDOlustur() + ".pdf");
                        formData.Fields["BELGEYOLU"].AsString = newFile.Path;
                        newFile.Upload(ustYazi);
                        form.AddRelation(newFile.Path, "default");
                        if (p.Ustveri.EkleriAl() != null)
                        {
                            foreach (var item in p.Ustveri.EkleriAl())
                            {
                                char[] sperator = new char[] { '?', '\\', ':', '*', '<', '>', '&', '|' };
                                //string replaceDosyaAdi = ReplaceYap(item.DosyaAdi, sperator, " ");
                                string replaceDosyaAdi = string.IsNullOrEmpty(item.DosyaAdi) ? "" : ReplaceYap(item.DosyaAdi, sperator, " ");
                                string ekTur = item.Tur.ToString();
                                WorkflowDocument doc = con.WorkflowManager.CreateDocument("GBK", "EK");
                                eBAForm ek = new eBAForm(doc.DocumentId);
                                ek.Fields["AD"].AsString = string.IsNullOrEmpty(replaceDosyaAdi) ? item.Ad.Value : replaceDosyaAdi;
                                ek.Fields["EKAD"].AsString = replaceDosyaAdi;
                                ek.Fields["TUR"].AsString = ekTur;
                                ek.Fields["EKTURU"].AsString = "1";
                                ek.Fields["BELGENO"].AsString = item.ImzaliMi ? string.IsNullOrEmpty(item.BelgeNo) ? "" : item.BelgeNo : "";
                                ek.Fields["ACIKLAMA"].AsString = (item.Aciklama != null ? item.Aciklama.Value : "");
                                ek.Fields["TUR_TEXT"].AsString = ekTur.Equals("DED") ? "Elektonik Belge" : ekTur.Equals("HRF") ? "Harici Referans" : "Fiziksel Nesne";
                                string ekPath = "workflow/GBK/EK/" + doc.DocumentId.ToString() + ".wfd";
                                if (item.Tur.ToString().Equals("DED"))
                                    fs.UploadFileAttachmentContentFromStream(ekPath, "default", replaceDosyaAdi, p.EkAl(Guid.Parse(item.Id.Value)));

                                ek.OwnerDocumentId = formData.Id;
                                ek.Update();
                                formData.Details["EKLER"].Rows.Add(doc.DocumentId);
                            }
                            formData.Update();
                        }
                        if (p.Ustveri.IlgileriAl() != null)
                        {
                            foreach (var item in p.Ustveri.IlgileriAl())
                            {
                                WorkflowDocument docIlgi = con.WorkflowManager.CreateDocument("GBK", "ILGI");
                                eBAForm Ilgi = new eBAForm(docIlgi.DocumentId);
                                Ilgi.Fields["BELGENO"].AsString = item.BelgeNo != null ? item.BelgeNo : "";
                                Ilgi.Fields["ACIKLAMA"].AsString = item.Aciklama != null ? item.Aciklama.Value : "";
                                DateTime result = new DateTime();
                                DateTime.TryParse(item.Tarih.ToString(), out result);
                                if (result.Year > 1001)
                                    Ilgi.Fields["TARIH"].AsDateTime = result;
                                string ekPath = "workflow/GBK/ILGI/" + docIlgi.DocumentId.ToString() + ".wfd";
                                Ilgi.Update();
                                formData.Details["ILGILER"].Rows.Add(docIlgi.DocumentId);
                            }
                            formData.Update();
                        }
                    }
                    else   //EYP 2.0 ise
                    {
                        Cbddo.eYazisma.Tipler.Paket p = Cbddo.eYazisma.Tipler.Paket.Ac(st, Cbddo.eYazisma.Tipler.PaketModu.Ac);
                        string BelgeNo = "";
                        //BelgeNo = p.Ustveri.BelgeNoAl().ToString();
                        //BelgeNo = p.NihaiUstVeri.BelgeNoAl().ToString();
                        formData.Fields["RUID"].AsString = p.Ustveri.BelgeIdAl().ToString();
                        formData.Fields["KONU"].AsString = p.Ustveri.KonuAl().Value;
                        formData.Fields["TARIH"].AsDateTime = p.NihaiUstveri.TarihAl();
                        formData.Fields["BELGENO"].AsString = p.NihaiUstveri.BelgeNoAl().ToString();
                        formData.Fields["GUVKOD"].AsString = p.Ustveri.GuvenlikKoduAl().ToString();
                        formData.Fields["GLSYOLU"].AsString = "KEP";
                        formData.Fields["GLSYOLU_TEXT"].AsString = "KEP";
                        filename = p.Ustveri.BelgeIdAl().ToString() + ".pdf";

                        if (p.Ustveri.OlusturanAl().Item.ToString() == "Cbddo.eYazisma.Xsd.CT_KurumKurulus")
                        {
                            formData.Fields["RDLDGT"].AsString = "KK";
                            formData.Fields["RDLDGT_TEXT"].AsString = "Kurum";
                            formData.Fields["OLUSTURANKURUM"].AsString = ((Cbddo.eYazisma.Xsd.CT_KurumKurulus)(p.Ustveri.OlusturanAl().Item)).KKK;
                            formData.Fields["OLUSTURANKURUM_TEXT"].AsString = ((Cbddo.eYazisma.Xsd.CT_KurumKurulus)(p.Ustveri.OlusturanAl().Item)).Adi.Value;
                            //string hiyerarsi =getHiyerarsi(((Dpt.eYazisma.Xsd.CT_KurumKurulus)(p.Ustveri.OlusturanAl().Item)).KKK,((Dpt.eYazisma.Xsd.CT_KurumKurulus)(p.Ustveri.OlusturanAl().Item)).Adi.Value);  
                            formData.Fields["TXTKURUM"].AsString = ((Cbddo.eYazisma.Xsd.CT_KurumKurulus)(p.Ustveri.OlusturanAl().Item)).Adi.Value;
                        }
                        if (p.Ustveri.OlusturanAl().Item.ToString() == "Cbddo.eYazisma.Xsd.CT_TuzelSahis")
                        {
                            formData.Fields["RDLDGT"].AsString = "TS";
                            formData.Fields["RDLDGT_TEXT"].AsString = "Tüzel Şahıs";
                            formData.Fields["TZLSHS"].AsString = ((Cbddo.eYazisma.Xsd.CT_TuzelSahis)(p.Ustveri.OlusturanAl().Item)).Id.Value;
                            formData.Fields["TZLSHSTEXT"].AsString = ((Cbddo.eYazisma.Xsd.CT_TuzelSahis)(p.Ustveri.OlusturanAl().Item)).Adi.Value;
                        }
                        if (p.Ustveri.OlusturanAl().Item.ToString() == "Cbddo.eYazisma.Xsd.CT_GercekSahis")
                        {
                            formData.Fields["RDLDGT"].AsString = "KO";
                            formData.Fields["RDLDGT_TEXT"].AsString = "Kişi";
                            string ilkAd = ((Cbddo.eYazisma.Xsd.CT_GercekSahis)(p.Ustveri.OlusturanAl().Item)).Kisi.IlkAdi.Value;
                            string ikinciAd = "";//((Dpt.eYazisma.Xsd.CT_GercekSahis)(p.Ustveri.OlusturanAl().Item)).Kisi.IkinciAdi.Value;
                            string soyad = ((Cbddo.eYazisma.Xsd.CT_GercekSahis)(p.Ustveri.OlusturanAl().Item)).Kisi.Soyadi.Value;
                            string fullName = string.Concat(ilkAd, " ", ikinciAd, " ", soyad);
                            formData.Fields["KISISEC"].AsString = fullName;
                            formData.Fields["KISISEC_TEXT"].AsString = fullName;
                        }

                        formData.Update();

                        System.IO.Stream ustYazi = p.UstYaziAl();

                        if (!con.FileSystem.HasFolder("SDP/Kep"))
                        {
                            con.FileSystem.CreateFolder("SDP/Kep");
                        }

                        DMFile form = fs.GetFile(Metadata.Path);
                        DMFile newFile = con.FileSystem.CreateFile("SDP/Kep/" + eBABYSHelper.BYSHelper.UIDOlustur() + ".pdf");
                        formData.Fields["BELGEYOLU"].AsString = newFile.Path;
                        newFile.Upload(ustYazi);
                        form.AddRelation(newFile.Path, "default");

                        if (p.Ustveri.EkleriAl() != null)
                        {
                            foreach (var item in p.Ustveri.EkleriAl())
                            {
                                string ekTur = item.Tur.ToString();
                                WorkflowDocument doc = con.WorkflowManager.CreateDocument("GBK", "EK");
                                eBAForm ek = new eBAForm(doc.DocumentId);
                                ek.Fields["AD"].AsString = string.IsNullOrEmpty(item.DosyaAdi) ? item.Ad.Value : item.DosyaAdi;
                                ek.Fields["EKAD"].AsString = item.DosyaAdi;
                                ek.Fields["TUR"].AsString = ekTur;
                                ek.Fields["BELGENO"].AsString = item.ImzaliMi ? string.IsNullOrEmpty(item.BelgeNo) ? "" : item.BelgeNo : "";
                                ek.Fields["ACIKLAMA"].AsString = (item.Aciklama != null ? item.Aciklama.Value : "");
                                ek.Fields["TUR_TEXT"].AsString = ekTur.Equals("DED") ? "Elektonik Belge" : ekTur.Equals("HRF") ? "Harici Referans" : "Fiziksel Nesne";
                                string ekPath = "workflow/GBK/EK/" + doc.DocumentId.ToString() + ".wfd";
                                if (item.Tur.ToString().Equals("DED"))
                                {
                                    fs.UploadFileAttachmentContentFromStream(ekPath, "default", item.DosyaAdi, p.EkAl(Guid.Parse(item.Id.Value)));
                                    ek.Fields["EKTURU"].AsInteger = 1;
                                }
                                ek.OwnerDocumentId = formData.Id;
                                ek.Update();
                                formData.Details["EKLER"].Rows.Add(doc.DocumentId);
                            }

                            formData.Update();
                        }

                        if (p.Ustveri.IlgileriAl() != null)
                        {
                            foreach (var item in p.Ustveri.IlgileriAl())
                            {
                                WorkflowDocument docIlgi = con.WorkflowManager.CreateDocument("GBK", "ILGI");
                                eBAForm Ilgi = new eBAForm(docIlgi.DocumentId);
                                Ilgi.Fields["BELGENO"].AsString = item.BelgeNo != null ? item.BelgeNo : "";
                                Ilgi.Fields["ACIKLAMA"].AsString = item.Aciklama != null ? item.Aciklama.Value : "";
                                DateTime result = new DateTime();
                                //DateTime.TryParse(item.Tarih.ToString(), out result);
                                if (result.Year > 1001)
                                    Ilgi.Fields["TARIH"].AsDateTime = result;
                                string ekPath = "workflow/GBK/ILGI/" + docIlgi.DocumentId.ToString() + ".wfd";
                                Ilgi.Update();
                                formData.Details["ILGILER"].Rows.Add(docIlgi.DocumentId);
                            }

                            formData.Update();
                        }
                    }
                }
                else
                {
                    //LoadKEPAttachs(varEMLPath.Value,Metadata.Path,"EMLAttachments","default"); 
                }
            }
            formData.Update();
        }
        else  //EYP Paketi Değil ise. 
        {
            LoadKEPAttachs(varEMLPath.Value, Metadata.Path, "EMLAttachments", "default");
        }
    }
    #endregion 

}


void EBATable()
{
    eBATable Table1 = new eBATable();

    DataRow dr = Table1.Data.Rows[0];
    int a = (int)dr["ORDERID"];

    dr["FIRSTNAME"] = "MyUser";
    dr["LASTNAME"] = "MyLastname";

    DataRow row = Table1.CreateRow();
    //row["CHECKED"] = "0";
    row["FIRSTNAME"] = "User";
    row["LASTNAME"] = "LastName";
    Table1.InsertRow(row);

    BaseForm frm = new BaseForm();
    frm.SaveFormData(false, true);
    frm.LoadData();

}

void DocumentHistory()
{
    documentHistoryBase Approvers1 = new documentHistoryBase();

    DataTable dt = GeneralDL.GetUserCustomProperties();
    List<string> vs = dt.AsEnumerable()
                    .Select(r => r.Field<string>("UserCode"))
                    .ToList();

    Approvers1.VisibleCustomProperties.AddRange(vs);

}

void MailAddAttAndRelDoc()
{
    //<#eBA Workflow Studio created code begin> -- do not remove
    eBAMail ebamail = new eBAMail();
    ebamail.TO.Add(FlowStarter1.Email);
    ebamail.Subject = @"Subject...";
    ebamail.Body = @"Message...";
    ebamail.Subject = FormatMessage(ebamail.Subject, false);
    ebamail.Body = FormatMessage(ebamail.Body, ebamail.IsHtml);
    eBAConnection con = CreateServerConnection();
    con.Open();
    try
    {
        FileSystem fs = con.FileSystem;
        string path = "workflow/MailView/Form/" + Document1.ProfileId + ".wfd";
        DMFile fl = fs.GetFile(path);
        //category1ViewA kategorisindeki attachmentları eklemek için
        foreach (DMFileContent cnt in fl.GetAttachments("category1ViewA"))
        {
            ebamail.BinaryAttachments.Add(cnt.ContentName, fs.DownloadFileAttachmentContentToByteArray(path, "category1ViewA", cnt.ContentName));
        }
        foreach (DMFileRelation cnt in fl.GetRelations())
        {
            ebamail.BinaryAttachments.Add(cnt.Path, fs.DownloadFileContentToByteArray(path, cnt.Path));
        }
        foreach (DMFileRelation dfr in fl.GetRelations())
        {

            eBAMail.AddAttachmentFromDMFileSystem(con, dfr.Path);

        }
    }
    catch
    {

    }


    eBAMail.Send();
    //<#eBA Workflow Studio created code end> -- do not remove

}

void UnauthorizedException()
{
    //using 
    #region Catch

    string filePath = @".\ROFile.txt";
    if (!File.Exists(filePath))
        File.Create(filePath);
    // Keep existing attributes, and set ReadOnly attribute.
    File.SetAttributes(filePath,
                      (new FileInfo(filePath)).Attributes | FileAttributes.ReadOnly);

    StreamWriter sw = null;
    try
    {
        sw = new StreamWriter(filePath);
        sw.Write("Test");
    }
    catch (UnauthorizedAccessException)
    {
        FileAttributes attr = (new FileInfo(filePath)).Attributes;
        Console.Write("UnAuthorizedAccessException: Unable to access file. ");
        if ((attr & FileAttributes.ReadOnly) > 0)
            Console.Write("The file is read-only.");
    }
    finally
    {
        if (sw != null) sw.Close();
    }
    #endregion
    /*


    */

    string path = @".\ROFile.txt";
    try
    {
        File.Delete(path);
    }
    catch (UnauthorizedAccessException)
    {
        FileAttributes att = File.GetAttributes(path);
        if ((att & FileAttributes.Normal) != FileAttributes.Normal) // File.GetAttributes(stringFileName) == FileAttributes.Normal
        {
            File.SetAttributes(path, FileAttributes.Normal);
        }


        else
        {
            throw;
        }
    }
    /*catch (UnauthorizedAccessException)
    {
        FileAttributes attributes = File.GetAttributes(path);
        if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
        {
            attributes &= ~FileAttributes.ReadOnly;
            File.SetAttributes(path, attributes);
            File.Delete(path);
        }
        else
        {
            throw;
        }
    }*/
}

void DocumentBuilder()
{
    using (eBAConnection con = CreateServerConnection())

    {

        con.Open();

        DMFile doc = con.FileSystem.GetFile("files/docbuilder HTML Metin Şablon.docx");
        DMFileObjectProperties oProp = doc.ObjectProperties;
        string pıd = oProp.Profile.HasValue ? oProp.Profile.Value.ToString() : "Değer Boş";

        DocumentBuilder wDoc = new DocumentBuilder();

        Wdoc.Items.AddText("GOVDEMETNI", HTML1.Text, true);
        //Html olarak göndermek için isHtml parametresi true olmalıdır

        Wdoc.Build(doc.Download());

        using (Stream respStream = new MemoryStream())

        {

            Wdoc.Save(respStream, SaveFormat.Pdf);

            respStream.Seek(0, SeekOrigin.Begin);



            DMFile target = con.FileSystem.CreateFile("files/" + " + id.ToString() + " + ".pdf");




            target.Upload(respStream);

            WriteToResponse(respStream, "OlusanWordIsmi.Pdf");

        }

    }
}

void Validate()
{
    // using System.Linq;

    string[] authors = { "Adana", "Adıyaman",
                        "Afyonkarahisar", "Ağrı" };
    DataTable dt = new DataTable();
    DataRow dr;

    dt.Columns.Add("sehir_TEXT");
    dt.Columns.Add("sehir");



    for (int i = 0; i < authors.Length; i++)
    {
        dr = dt.NewRow();
        dr["sehir_TEXT"] = i;
        dr["sehir"] = authors[i];
        dt.Rows.Add(dr);
    }




    eBAForm frm = new eBAForm(Id);



    string expression = "lstCity_TEXT = " + frm.Fields["lstCity_TEXT"].AsString + "";

    DataView dataView = new DataView(dt);
    dataView.RowFilter = expression;

    int a = dataView.Count;

    if (a > 0)
    {
        summary.AddMessage("Lütfen Türkiye için geçerli bir şehir seçin");
    }

    /*foreach (DataRow item in )
    {


        if (item["lstCity_TEXT"].))
        {
            //  summary.AddMessage("Lütfen Türkiye için geçerli bir şehir seçin");

        };

    }*/

    frm.Fields["Liste1_TEXT"].AsString = "İstanbul";
    frm.Fields["Liste1"].AsString = "";
}

void FlowState()
{
    string a = FlowStatus.Rejected.ToString();
}

void IletisimBilgileri(int Id, eBAForm frm, string RDLDGTtip)
{
    try
    {
        //DTVTKEP localde yok

        //if(!string.IsNullOrEmpty(detsisId.ToString()))
        if (RDLDGTtip == "KK")    //detsisId üzerinden  - Kurum
        {
            Dictionary<string, string> dict = new Dictionary<string, string>
                    {
                        { "ARA", Id.ToString() }
                    };
            DataTable dt = getDataFromIntegration("EBYS", "EBYS_DTVTKEP", dict);     //Id,İsim,Hiyerarşik ile arama yapılabilir
                                                                                     //datatable içerisinde,integration managerdeki sorgudan gelen tablolar içerisindeki sütunlar -> TELEFON,EPOSTA,FAKS,WEBADRESI,KEP,ADRES

            if (dt.Rows.Count > 0)
            {
                frm.Fields["TELEFON"].AsString = dt.Rows[0]["TELEFON"].ToString();
                frm.Fields["EPOSTA"].AsString = dt.Rows[0]["EPOSTA"].ToString();
                frm.Fields["FAKS"].AsString = dt.Rows[0]["FAKS"].ToString();
                frm.Fields["WEBADR"].AsString = dt.Rows[0]["WEBADRESI"].ToString();
                frm.Fields["KEP"].AsString = dt.Rows[0]["KEP"].ToString();           //Sql'de Mevcut
                frm.Fields["KURUMADRES"].AsString = dt.Rows[0]["ADRES"].ToString();  //Sql'de Mevcut 
            }
        }

        else if (RDLDGTtip == "TS")     //MersisNO üzerinden -Tüzel Şahıs
        {
            Dictionary<string, string> dict = new Dictionary<string, string>
                    {
                        { "ARA", Id.ToString() }
                    };
            DataTable dt = getDataFromIntegration("EBYS", "EBYS_TUZELSAHIS", dict); //İntegrationManager Oluşturulacak

            if (dt.Rows.Count > 0)
            {
                frm.Fields["TELEFON"].AsString = ""; // dt.Rows[0]["TELEFON"].ToString();
                frm.Fields["EPOSTA"].AsString = ""; // dt.Rows[0]["EPOSTA"].ToString();
                frm.Fields["FAKS"].AsString = ""; // dt.Rows[0]["FAKS"].ToString();
                frm.Fields["WEBADR"].AsString = ""; // dt.Rows[0]["WEBADRESI"].ToString();
                frm.Fields["KEP"].AsString = ""; // dt.Rows[0]["KEP"].ToString();           //Sql'de Mevcut
                frm.Fields["KURUMADRES"].AsString = ""; // dt.Rows[0]["ADRES"].ToString();  //Sql'de Mevcut 
            }
        }

        else if (RDLDGTtip == "KO")    //Kişinin Id'si olacak, - Kişi 
        {
            Dictionary<string, string> dict = new Dictionary<string, string>
                    {
                        { "ARA", Id.ToString() } //KisiSec Id' sinden ADSOYAD geliyor      
                    };
            DataTable dt = getDataFromIntegration("EBYS", "EBYS_DTVTKEP", dict);  //İntegrationManager Oluşturulacak

            if (dt.Rows.Count > 0)
            {
                frm.Fields["TELEFON"].AsString = ""; // dt.Rows[0]["TELEFON"].ToString();
                frm.Fields["EPOSTA"].AsString = ""; // dt.Rows[0]["EPOSTA"].ToString();
                frm.Fields["FAKS"].AsString = ""; // dt.Rows[0]["FAKS"].ToString();
                frm.Fields["WEBADR"].AsString = ""; // dt.Rows[0]["WEBADRESI"].ToString();
                frm.Fields["KEP"].AsString = ""; // dt.Rows[0]["KEP"].ToString();           //Sql'de Mevcut
                frm.Fields["KURUMADRES"].AsString = ""; // dt.Rows[0]["ADRES"].ToString();  //Sql'de Mevcut 

            }
        }

        frm.Update();
    }
    catch (Exception ex)
    {
        //throw new Exception("DetsisID: "+detsisId.ToString()); 
    }

}

void FlowPosition()
{
    FlowPosition pos_Ismi = new FlowPosition(new BaseFlowCode(), "");

    pos_Ismi.SetEventEnable(5, true); //5; onay eventi
                                      //pos_Ismi.
}

void ConfirmTest()
{

    //%SystemPath%\Common\ebanet.dll

    //using ebanet;
    RequestEvent request = new RequestEvent
    {
        Icon = "s",
        Text = "Yeni Olay",
        Confirm = true
    };

    RequestEvents events = new RequestEvents();

    events.Add(request);





}

void eBAFlowScrpAdp()
{
    FlowMail flow = new FlowMail(new BaseFlowCode(), "");

}

void DmObjectSecurityDeneme()
{
    #region DepartmanBazlı

    //using eBAPI.DocumentManagement.Security;
    eBAConnection eCon = CreateServerConnection();//CreateServerConnection()

    try
    {
        eCon.Open();

        string path = "workspace/users";
        string departman = string.Empty;

        DMObjectSecurity pr = eCon.FileSystem.Security.GetFolderSecurity(path);
        DMObjectSecurityPermission spr = pr.AddPermission(DMPermissionRoleType.DepartmentRole, "DEP001");

        spr[DMPermissionType.Publish] = DMPermissionStatus.Allow;
        spr[DMPermissionType.ShareLink] = DMPermissionStatus.Deny; //Harici link
        spr[DMPermissionType.CreateFile] = DMPermissionStatus.Deny;
        //spr[DMPermissionType.EditSecurity] = DMPermissionStatus.Allow;

        eCon.FileSystem.Security.SetFolderSecurity(path, pr);


    }
    catch (Exception)
    {

        throw;
    }
    finally
    {
        eCon.Close();
    }

    #endregion

    #region UserBazlı

    //using eBAPI.DocumentManagement.Security;
    //eBAConnection eCon = CreateServerConnection();//CreateServerConnection()

    try
    {
        eCon.Open();

        string path = "workspace/users";
        string departman = string.Empty;

        DMObjectSecurity pr = eCon.FileSystem.Security.GetFolderSecurity(path);
        DMObjectSecurityPermission spr = pr.AddPermission(DMPermissionRoleType.UserRole, "adogru");

        spr[DMPermissionType.Publish] = DMPermissionStatus.Allow;
        spr[DMPermissionType.ShareLink] = DMPermissionStatus.Allow;

        eCon.FileSystem.Security.SetFolderSecurity(path, pr);
    }
    catch (Exception)
    {

        throw;
    }
    finally
    {
        eCon.Close();
    }

    #endregion
    //using eBAPI.DocumentManagement.Security;
    eBAConnection eCon = CreateServerConnection();//CreateServerConnection()

    try
    {
        eCon.Open();

        string path = "workspace/users";
        string departman = string.Empty;

        DMObjectSecurity pr = eCon.FileSystem.Security.GetFolderSecurity(path);
        DMObjectSecurityPermission spr = pr.AddPermission(DMPermissionRoleType.UserRole, "adogru");

        spr[DMPermissionType.Publish] = DMPermissionStatus.Allow;
        spr[DMPermissionType.ShareLink] = DMPermissionStatus.Allow;
    }
    catch (Exception)
    {

        throw;
    }
    finally
    {
        eCon.Close();
    }


    //DMPermissionType.ShareLink = DMPermissionStatus.Deny;
}

void ComboBoxDeneme()
{
    //using System.Collections.Generic;
    eBAComboBox cmb = new eBAComboBox();
    cmb.AutocompleteEnable = true;
    if (cmb.Value == " ")
    {

    }

    /* List<string> strValue = cmb.ValueFields;
     List<string> strText= cmb.TextFields;

     cmb.Value = strValue[1];
     cmb.Text = strText[1];*/ //0'da Sütunun Değerini Aldı.

    string[] strValue = cmb.ValueFields.ToArray();
    string[] strText = cmb.TextFields.ToArray();

    cmb.Value = strValue[1];
    cmb.Text = strText[1];

    foreach (var item in cmb.ValueFields.ToArray())
    {

    }

    eBAComboBox lstCounty = new eBAComboBox();
    eBAComboBox lstCity = new eBAComboBox();
    //List<string> sehirler = new List<string>();

    string[] authors = { "Adana", "Adıyaman",
                        "Afyonkarahisar", "Ağrı" };
    DataTable dt = new DataTable();

    dt.Rows.Add(authors);


    foreach (var item in dt.Rows)
    {
        if (!lstCity.Text.Equals(item))
        {
            ShowMessageBox("Lütfen Türkiye için geçerli bir şehir seçin");

        };

    }
    lstCity.Text = "İstanbul";


}

static void Holidays()
{
    // Çalışma saatleri dikkate alınması için  =CalculationOption.WorkingHours
    // Geri Dönüş değeri int
    // Eğer buçuklu bir çıktı alınmak istenirse GetMinutes metodu ile Dakika üzerinden saat ve çalışma süresine bölünüp bulunabilir

    DateTime startDate = new DateTime(2021, 08, 11, 8, 30, 00);
    DateTime endDate = new DateTime(2021, 08, 12, 16, 30, 00);

    //int a = WorkDayCalculator.GetDays(startDate, endDate, CalculationOption.WorkingHours, false, null);
    //int minutes = WorkDayCalculator.GetMinutes(startDate, endDate, CalculationOption.WorkingHours, false, null);
    int minutes = 1320;
    int hour = minutes / 60;
    int gun = 22 / 9;       //9; Çalışma süresi
    int yarımgun = 22 % 9;   //yarımgun; hafta sonu çalışma süresi

    Console.WriteLine(gun + "," + yarımgun);
}

void SetView()
{
    /*public void SDAGITIMKISI_SelectedIndexChanged(object sender, EventArgs e)
    {
        SetView();
    }*/

    if (AskiId > 0)
    {
        switch (SDAGITIMKISI.SelectedValue)
        {
            case "KK":
                CurrentView = "default";

                //    Clear();
                break;
            case "KO":
                CurrentView = "Kisi";
                //   Clear();
                break;
            case "TS":
                CurrentView = "TuzelSahis";
                //      Clear();
                break;
        }
    }

}

void DetaylarUpdate(eBAForm source, eBAForm target, string field)
{
    target.Fields[field].AsString = source.Fields[field].AsString;
}

void DetailsCheck()
{
    DataTable data = Details1.Data;
    for (int i = 0; i < data.Rows.Count; i++)
    {

        Text1.Text = data.Rows[i]["txtTutar"].ToString();
    }
    Details1.Data = data;

    /*public void Details1_RowCheck(object sender, bool state, DataRow dr)
    {
        if (state)
        {
            foreach (DataRow drow in Details1.Data.Rows)
            {
                if (drow != dr)
                {
                    drow["CHECKED"] = "0";
                }
            }
        }
    }*/

    /*
    int rowcount = 0;

    foreach (DataRow drow in Detaylar1.Data.Rows)
    {
        if (drow["CHECKED"].ToString() == "1")
        {
            rowcount++;
        }
    }
    Text1.Text = rowcount.ToString();
    */

}

void Test()
{
    getProcessParameters("ProcessName");
}

void relDocPathControl()
{
    //OnBeforeRelation event
    if (string.IsNullOrEmpty(Path.GetExtension(e.Filename)))
    {
        ShowMessageBox("Dosyanın tipi belirlenemedi. Uzantısız dosya yükleyemezsiniz.");
        e.allow = false;
    }
    if (!(Path.GetExtension(e.Filename) == ".docx" || Path.GetExtension(e.Filename) == ".doc" || Path.GetExtension(e.Filename) == ".xlsx" || Path.GetExtension(e.Filename) == ".xls" || Path.GetExtension(e.Filename) == ".pdf"))
    {
        ShowMessageBox("Dosya uzantısı geçersiz");
        e.allow = false;
    }
}

void ExecuteNonQuery(string query)
{
    SqlConnection sqlCon = (SqlConnection)CreateDatabaseConnection();
    sqlCon.Open();

    try
    {
        SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
        sqlCmd.ExecuteNonQuery();
    }
    catch (Exception ex)
    {
        throw new Exception("Sql sorgusu çalışırken hata oluştu: " + ex.Message);
    }
    finally
    {
        sqlCon.Close();
    }
}

DataTable getDataFromIntegration(string connection, string query, Dictionary<string, string> prm)
{

    // using eBAIntegrationAPI;
    DataTable dt;

    eBAConnection con = CreateServerConnection();

    try
    {
        con.Open();
        eBAIntegrationQuery sorgu = new eBAIntegrationQuery(connection, query);
        /*foreach (KeyValuePair<string, string> p in prm)
        {
            sorgu.Parameters.Add(p.Key, p.Value);
        }*/
        dt = sorgu.Execute(con);
        /*if (dt.Columns.Contains("TEXT"))
        {
            dt.Columns.Remove("TEXT");
        }*/
        return dt;

    }
    finally
    {
        con.Close();
    }
}

void EbaLogHelper()
{
    //using eBALogAPIHelper.Helper;
    string LogonUser;

    eBALogAPI eBALog = ebanet.eBALog.CreateLogAPI(""); // new eBALogAPI(applicationName:"",instance:"");
    eBALog.ResumeOnException = true;

    eBALog.AddLogAsync(
               logText: "",
               logDetailsText: "",
               eBALogType.None,
               userId: LogonUser,
               exception: ex);

    try
    {

    }
    catch (Exception ex)
    {
        eBALog.AddLogAsync(
                logText: "",
                logDetailsText: "",
                eBALogType.Error,
                userId: LogonUser,
                exception: ex);
    }
    finally
    {

    }

}

void eBAContext()
{
    eBAForm frm = new eBAForm(1);
    DataTable dt = eBADataContext.Connection.GetDataTable("select * from KULLANICIYETKI where KLNC='" + AkisiBaslatan1.User + "'");
    FormTable ft = frm.Tables[""];
    DataRow row1 = dt.Rows[0];

    foreach (DataRow row in dt.Rows)
    {
        FormTableRow tableRow = ft.Rows.Add();
        tableRow[""].AsString = row[""].ToString();
    }

    int numb = eBADataContext.Connection.ExecuteCommand("sql");
    var variable = eBADataContext.Connection.GetScalarValue("sql");
    eBADataContext.Connection.ExecuteNonQuery("sql");
}

void eBADBHelper()
{
    int frmid = GeneralDL.HasParametersFormRecord("EBYSPRM", "CONTACTLIST"); //Parametre Formu Oluşmuş mu

    DataTable dt = CaptureDL.GetDataTable("query",
        new List<StringParameter>() {
                    new StringParameter("", "")
        });

    DataTable dt2 = GeneralDL.GetUserManagers(args.UserId); //args pozisyon nesnesinin eventi

}

void formRibbonGroupButtonVisibilty(bool visible, string groupName, string buttonName)
{
    RibbonGroup formGroup = RibbonBar.FindGroup(groupName);
    if (formGroup != null)
    {
        RibbonButton formButton = RibbonBar.FindButton(groupName, buttonName);
        if (formButton != null)
        {
            formButton.Visible = visible;
        }
    }
}

void ShowModalDocument()
{
    //Table Row Selected Eventi //dr; DataRow
    ShowModalDocument(RBTABLE, dr["BELGEYOL"].ToString(), false);
}

void rollBackProcess(string processId, string step)
{
    eBAConnection con = CreateServerConnection();
    try
    {
        con.Open();
        if (Convert.ToInt32(step) > 0)
        {
            eBASystemAPI.Utils.RollbackFlow(con, Convert.ToInt32(processId), Convert.ToInt32(step));
        }
    }
    catch (Exception ex)
    {
        throw new Exception(ex.Message + "-" + processId + "-" + step);
    }
    finally
    {
        con.Close();
    }

}

void DetailsControl()
{
    eBAForm m = new eBAForm(id); //FormId
    if (m.Details["CLIST"].Rows.Any(a => a.Form.Fields["KULLANICI"].AsString == LogonUser))
    {
        formRibbonGroupButtonVisibilty(false, "EventButtons", "Onayla");
        ShowMessageBox("Kayıtlı Telefon Numaranız Bulunmaktadır");
    }
}

void DelRelDoc()
{

    //DMFile fl = fs.GetFile("workflow/EtiketOnaySureci/Form/" + id.ToString() + ".wfd"); //bulunduğumuz formu alıyoruz. FormID
    eBAConnection con = CreateServerConnection();
    con.Open();

    FileSystem fs = con.FileSystem;
    DMFile fl = fs.GetWorkflowFile(id); //Sileceğimiz İlişkili Dokümanlar nesnesinin form id si

    try
    {

        foreach (DMFileRelation docDel in fl.GetRelations())
        {
            fs.DeleteFile(docDel.Path);
        }
    }
    catch (Exception ex)
    {
        throw new Exception(" An error ocured while deleting attachments!\n" + ex.Message);
    }
    finally
    {
        con.Close();
    }
}

void AddWaterMark()
{

    eBAConnection con = CreateServerConnection();
    con.Server = "PRODUCTION";
    con.UserID = "admin";
    con.Password = "0";
    con.Open();

    try
    {
        con.Open();
        FileSystem fs = con.FileSystem;

        DMFile pdf = fs.GetFile("Sınav Dosyaları/Talep Formu/[50].pdf"); //targetPath
        DMFile filigramPath = fs.GetFile("files/Filigram.pdf"); // SourcePath

        //Forma Kod ile watermark ekleme
        string filePath = "workflow/ProjectSubFlow/Form/196.wfd";
        string path = "files/Watermark1.pdf";
        PDFExport ebapdfexport = new PDFExport(con);

        ebapdfexport.AddDocument(filePath,);
        ebapdfexport.SetWatermark("sample.jpg");



        ebapdfexport.Export();

        ebapdfexport.SaveToDMFileSystem(path);

        PDFUtils.AddWaterMarkToDmFile(
        con,
        filigramPath.Path,
        pdf.Path,
        "sample.jpg", 50, 45, 1, true);

        /*Metot Bilgisi:
         * 
         watermark: "system/images/watermarks/" Klasörü altındaki damga
         */
    }
    finally
    {
        con.Close();
    }

}

void eBADbProvider()
{

    eBADBProvider db = CreateDatabaseProvider();
    try
    {
        db.Open();
        //getProcessParameters
        var pid = db.GetScalarValue("Select PROCESSID FROM FLOWDOCUMENTS where FILEPROFILEID='" + id.ToString() + "'");
        if (pid != null)
        {
            int akisId = Convert.ToInt32(pid.ToString());
        }
    }
    finally
    {
        db.Close();
    }
}

void AddNote()
{
    SqlConnection scon = (SqlConnection)CreateDatabaseConnection();
    scon.Open();

    string path = @"workflow/FormAndDMOperations/Form/" + id.ToString() + ".wfd";

    try
    {
        string sql = "INSERT INTO DOCUMENTNOTES (ID,DOCUMENTPATH,CREATORUSERID,CREATEDATE,MESSAGE,DELETED)VALUES((SELECT ABS(CAST(CAST(NEWID() AS VARBINARY) AS INT))),'" + path + "','" + Organization.GetPosition(LogonUser).Description.ToString() + "',GETDATE(),'" + txtEklenecekNot.Text + "',0)";

        //Position position = Organization.GetPosition(LogonUser);

        SqlCommand com = new SqlCommand(sql, scon);
        com.ExecuteNonQuery();

    }
    catch (Exception ex)
    {
        throw ex;
    }
    finally
    {
        scon.Close();
    }
}

void AddDmFromRelatedWithByteArrayAndSenkronize() //Tamamlanmadı
{   //if(rel.Category=="default") //formda birden fazla relations varsa category kontrol ediyoruz.
    //String path = rel.Path; // böyle pathi alıp aşağıda DMFile classına gidip streamini alabilirsiniz
    //DMFile doc = fs.GetFile(path);
    //Stream str = doc.Download(); 
    //flAtilacakAttachment.UploadAttachmentContentFromStream("default", doc.Content.Name, str);

    eBAForm anaForm = new eBAForm(Convert.ToInt32(AnaFormId.Value));//Ana Süreçteki Form
    eBAForm altForm = Document1.ProfileData; //Related Document ekleyeceğimiz Form

    eBAConnection con = CreateServerConnection();
    try
    {
        con.Open();
        FileSystem fs = con.FileSystem;

        DMFile flAtilacakRelations = fs.GetFile(altForm.DocumentPath);
        DMFileRelationCollection relation = fs.GetFileRelations(anaForm.DocumentPath);

        //DMFile form = fs.GetFile("workflow/FormAndDMOperations/Form/" + id.ToString() + ".wfd"); //Document1.Path

        //string categoryName = "default";  //  Category Name
        string targetFolder = "Sınav Dosyaları";
        foreach (DMFileRelation rel in relation)   // Category ismine göre attachment ları getiriyoruz
        {
            DMFile file = fs.GetFile(rel.Path);
            string contentName = file.Content.Name;

            if (!fs.HasLibrary(""))  //Library yoksa oluşturuyoruz
                fs.CreateLibrary("Verilen Eğitimler");


            if (!fs.HasFolder(targetFolder)) // Klasör yoksa Oluşturuyoruz
                fs.CreateFolder(targetFolder);

            string fileFullPath = targetFolder + "/" + contentName;
            if (!fs.HasFile(fileFullPath))                        // Dosyamız yoksa ekliyoruz  
            {
                DMFile att = fs.CreateFile(fileFullPath);
                byte[] byteArray = file.DownloadContentToByteArray(contentName);
                att.UploadContentFromByteArray(contentName, byteArray);
            }

            flAtilacakRelations.AddRelation(rel.Path, "default");
        }


        /* string categoryName = "default";
         string attName = rel.Category;
         if (!fs.HasFolder(targetFolder)) // Klasör yoksa Oluşturuyoruz
             fs.CreateFolder(targetFolder);

         string fileFullPath = targetFolder + "/" + attName;
         if (!fs.HasFile(fileFullPath))                        // Dosyamız yoksa ekliyoruz  
         {
             DMFile att = fs.CreateFile(fileFullPath);
             att.UploadContentFromByteArray(flAlinacakRelations.DownloadAttachmentContentBytes(categoryName, fileFullPath)); //Ana Form üzerindeki Dökümanları Dm'e Ekledik
         }
         */
        //Ana Form Üzerindeki Dökümanlar Tetiklenen Süreçteki İlişkili Dökümanlara nesnesine ekledik 
    }

    finally
    {
        con.Close();
    }

}

void DmFromRelatedWithByteArray() //Hatalı
{

    eBAConnection con = CreateServerConnection();
    try
    {
        con.Open();
        FileSystem fs = con.FileSystem;

        //DMFile form = fs.GetFile("workflow/FormAndDMOperations/Form/"+ id.ToString() +".wfd"); //Document1.Path
        DMFileRelationCollection relation = fs.GetFileRelations("workflow/FormAndDMOperations/Form/" + id.ToString() + ".wfd");

        //string categoryName = "default";  //  Category Name
        string targetFolder = "Sınav Dosyaları";
        foreach (DMFileRelation dmfr in relation)   // Category ismine göre attachment ları getiriyoruz
        {
            DMFile file = fs.GetFile(dmfr.Path);
            string contentName = file.Content.Name;

            if (!fs.HasFolder(targetFolder)) // Klasör yoksa Oluşturuyoruz
                fs.CreateFolder(targetFolder);

            string fileFullPath = targetFolder + "/" + contentName;
            if (!fs.HasFile(fileFullPath))                        // Dosyamız yoksa ekliyoruz  
            {
                DMFile att = fs.CreateFile(fileFullPath);
                byte[] byteArray = file.DownloadContentToByteArray(contentName);
                att.UploadContentFromByteArray(contentName, byteArray);
            }
        }

    }
    finally
    {
        con.Close();
    }

}

void DmFromLocal()
{
    eBAConnection con = new eBAConnection();
    con.Open();
    try
    {
        //string destPath = ""; //
        string localPath = @"C:\\Dosya DM\\"; //Localde dosyayı alacağımız yer
        FileSystem fs = con.FileSystem;
        fs.CreateFolder("DENEME TAHTASI/DMLOCAL2");  //DMde dosyayı atacağımız yer. Yeni klasör oluşturuyouz Processid isminde
        foreach (string file in Directory.GetFiles(localPath))   //Localdeki klasörde geziyoruz.
        {
            string[] filename = file.Split('\\');
            DMFile tempFile = fs.CreateFile("DENEME TAHTASI/DMLOCAL2/" + filename[filename.Length - 1]);
            tempFile.UploadContentFromFile(file); //dosyaları atıyoruz DMe

        }
    }
    finally
    {
        con.Close();
    }
}

void relDocRename()
{
    string fileName = FileSystem.GetFileName(e.Filename); //e: Related Documents On After Relation 
    eBAConnection con = CreateServerConnection();
    con.Open();
    FileSystem fs = con.FileSystem;
    DMFile fl = fs.GetFile("workflow/EtiketOnaySureci/Form/" + id.ToString() + ".wfd"); //bulunduğumuz formu alıyoruz. FormID
    DMFile fl2 = fs.GetWorkflowFile(id); //bulunduğumuz formu alıyoruz. FormID
    foreach (DMFileRelation rel in fl.GetRelations())
    {
        DMFile file = fs.GetFile(rel.Path);
        string a = fl.Name;

        string rename = "deneme" + "." + file.Content.Extension;
        fs.DeleteFile(file.Path);
    }
}

/// <summary>
/// Detaylar Ana Formda Validasyon Bölümü
/// Detaylar tablosunda oluşturanın silebilmesi
/// Detaylar Boş mu Kontrolü
/// </summary>
static void creatorDetails()
{
    eBAForm f = new eBAForm(Id);
    FormDetails fd = f.Details["DT"]; // detaylar nesnesinin adını yazıyorsunuz

    //string a = fd.Rows[0].ToString();

    string DtValue = fd.Rows.Count.ToString();

    if (string.IsNullOrEmpty(DtValue) || Convert.ToInt32(DtValue) == 0)
    {

    }


    // Details Row Deleting Eventi
    // Detay tablosunda olusturanın id sini tutacak gizli bir bölüm ekleyebiliriz
    if (dr["Id"].ToString() != LogonUser)
    {
        args.allow = false;
    }

}

void DmToAtt()
{
    eBAConnection con = CreateServerConnection();
    con.Open();

    try
    {
        FileSystem fs = con.FileSystem;
        DMFile doc = fs.GetFile("SınavDosyaları/TalepFormu/[50].pdf");   //Dmdeki Dosya Uzantısı
        DMFile form = fs.GetFile("workflow/FormAndDmOperations/Form/" + id.ToString() + ".wfd");                //Formun Pathi
        form.UploadAttachmentContentFromStream(category: "default", name: "[50].pdf", stream: doc.Download());

    }
    finally
    {
        //RefreshAttachments(Attachment1);
        con.Close();
    }
}

void LocalToAtt()
{
    eBAConnection con = CreateServerConnection();

    con.Open();
    try
    {
        string localPath = @"C:\\eba.net\\Dosyalar\\Attach\\"; //Localdeki Uzantı
        FileSystem fs = con.FileSystem;
        DMFile fl = fs.GetFile("workflow/D2017_4/Form/" + Dokuman1.ProfileId.ToString() + ".wfd");//Ekleyeceğimiz Attachment'ın Formu
        foreach (string file in Directory.GetFiles(localPath))
        {
            string[] filename = file.Split('\\');
            fl.UploadAttachmentContentFromFile("default", filename[filename.Length - 1], file);
        }
    }
    finally
    {
        con.Close();
    }
}

void DMToLocal()
{

    eBAConnection con = CreateServerConnection();
    string path = @"workflow/ProjectSubFlow/Form/196.wfd";
    try
    {
        con.Open();
        FileSystem fs = con.FileSystem;

        string localPath = @"C:\\eba.net\\Dosyalar\\";

        System.IO.Directory.CreateDirectory(localPath);
        fs.DownloadFileContentToFile(path: path, localFilePath: localPath + "\\" + "196.wfd"); //Dmdeki Dosyayı Locale Yükleme
                                                                                               //Path: Dm deki Dosya uzantısı
                                                                                               //LocalFilePath: Locale İndireceğimiz Uzantı

    }
    catch (Exception ex)
    {
        throw new Exception("An error ocured while copying attachments!\n" + ex.Message);
    }
    finally
    {
        con.Close();
    }

}

void Delegate()
{
    /*
        Metod Bilgi:
        Delegations.CreateDelegation(string delegatedFrom, int delegatedFromType, string delegatedTo, string process, System.DateTime startDate, System.DateTime ? expireDate) biçimindedir.Burada:

        DELEGATEDTO = Vekaleti alan kullanıcı(USERID)
        DELEGATEDFROM = Vekaleti veren kullanıcı(USERID)
        PROCESS = Vekalet verilecek süreç adı(proje adı), tam vekalet verilecek ise null olacak
        STARTDATE = vekalet başlangıç tarihi
        EXPIREDDATE = vekalet bitiş tarihi, süresiz vekalet verilecek ise null olacak
        DELEDATEDFROMTYPE = 0; Tam vekalet, süreç bazlı vekalet
     */
    eBAConnection eBACon = CreateServerConnection(); // yukarıya ebapi, ebapı connectıon ve ebalibraryler eklenir
    try
    {
        eBACon.Open();
        string delegatedFrom = "delegasyon verenin sicili";
        string delegatedTo = "delegasyon verilenin sicili";
        DateTime startDate = new DateTime(year: 2021, month: 08, day: 10); //Vekalet başlangıç tarihi 
        DateTime endDate = new DateTime(year: 2021, month: 08, day: 11, hour: 19, minute: 30, second: 30); //vekalet bitiş tarihi 
        Delegations.CreateDelegation(con: eBACon, delegatedFrom: delegatedFrom, delegatedFromType: 0, delegatedTo: delegatedTo, process: "", startDate: startDate, expireDate: endDate);// ""process ismi eğer //process bazlı verelecekse
    }
    finally
    {
        eBACon.Close();
    }
}

void savaPdfToDm()
{
    //<#eBA Workflow Studio created code begin> -- do not remove
    eBAConnection con = CreateServerConnection();
    con.Open();
    FileSystem fs = con.FileSystem;

    try
    {
        PDFExport ebapdfexport = new PDFExport(con);

        //Documents to export
        ebapdfexport.AddDocument(Document1.Path);
        ebapdfexport.Export();

        //Save exported document
        string libraryName = vCompany.Value;

        if (!fs.HasLibrary(libraryName))  //Library yoksa oluşturuyoruz
            fs.CreateLibrary(libraryName);

        string foldername = vCompany.Value + "/" + vDepartment.Value;

        if (!fs.HasFolder(foldername)) // Klasör yoksa Oluşturuyoruz 
            fs.CreateFolder(foldername);

        string filename = foldername + "/" + vTcNoValue + ".pdf";

        DMFile pdfFile = fs.GetFile(filename);
        pdfFile.AddRelation(Document2.Path, "default");  //ilişkili dosya ekliyoruz
        pdfFile.AddRelation(Document3.Path, "default");

        ebapdfexport.SaveToDMFileSystem(filename);
    }
    finally
    {
        con.Close();
    }

}

static void savePdf()
{
    //SaveFormData(false, true);
    eBAConnection con = CreateServerConnection();
    con.Open();
    PDFExport ebapdfexport = new PDFExport(con);
    try
    {
        ebapdfexport.AddDocument("workflow/SurecGelistirmeYonetimi/Form/" + id + ".wfd", "YoneticiView");
        ebapdfexport.SetPageSize(PageTypes.A4);
        ebapdfexport.SetPageMargins(45, 25, 25, 45);
        ebapdfexport.Export();

        //System.IO.MemoryStream mstream = ebapdfexport.SaveToStream();
        //byte[] byteArray = mstream.ToArray();
        //eBALibrary.Utils.SendDataToResponse(byteArray, "D2017_4" + id + ".pdf");

        eBAForm frm = Document1.ProfileData;

        string folderPath = "Sınav Dosyaları/Analiz Formu/Ekler";
        string filePath = "Sınav Dosyaları/Analiz Formu/Ekler/[" + frm.Fields["txtTalepNo"].Value + "].pdf";

        FileSystem fs = con.FileSystem;

        DMFile fl = fs.GetFile(filePath);

        if (!fs.HasFolder(folderPath)) // Klasör yoksa Oluşturuyoruz //Oluşturamadı
            fs.CreateFolder(folderPath);
        fs.CreateLibrary("");

        ebapdfexport.SaveToDMFileSystem(filePath);
        //ebapdfexport.SaveToLocalFileSystem(filePath);//Locale Kaydeder



    }
    catch (Exception ex)
    {
        throw new Exception("Kaydedilirken Hata Oluştu..." + " " + ex.Message);
    }
    finally
    {
        con.Close();
    }

}

/// <summary>
/// District Old Dates
/// </summary>
void setDateTime()
{

    if (txtBeklenenBitisTarihi.Value != null)
    {
        if (txtBeklenenBitisTarihi.Value <= DateTime.Now)
        {
            ShowMessageBox("Eski Tarihi Seçemezsiniz");

            txtBeklenenBitisTarihi.Clear();
        }
    }

}

void SetFlowPauser()
{
    eBAForm frm = Document1.ProfileData;

    // string a  = frm.Details["DT"].Rows[0][0].AsString;

    DateTime BitisTarihi = frm.Fields["txtSozlesmeBitisTarihi"].AsDateTime;

    TimeSpan ts = BitisTarihi - DateTime.Now; //Double Döner

    int beklemeSuresi = Convert.ToInt32(ts.TotalDays);

    ebaflow.SetFlowObjectValue("FlowPauser2.Day", (beklemeSuresi - 7).ToString()); //Kendi Sınıfı

}

void ApprovelStatusAndContinueMainFlow()
{
    eBAForm anaForm = Document2.ProfileData;

    int satirNo = Convert.ToInt32(TabloSatirNo.Value); // Alt akışı başlatan satırın no su

    anaForm.Tables["tblGozdengecirme"].Rows[satirNo]["DURUM"].AsString = "TAMAM"; // Db den çekilmemiş Durum satırı oluşturduk
    anaForm.Update();

    bool isOk = true;
    //Eğer Tüm durumlar tamam ise process onay olarak ilerliyecek
    foreach (var row in anaForm.Tables["tblGozdengecirme"].Rows)
    {
        if (row["DURUM"].AsString != "TAMAM")
        {
            isOk = false;
        }
    }

    if (isOk)
    {
        using (eBAConnection con = CreateServerConnection())
        {
            con.Open();

            WorkflowManager mgr = con.WorkflowManager;

            WorkflowProcess pr = mgr.GetProcess(processId: Convert.ToInt32(AnaAkisId.Value));

            //evenId onayla ya da reddet gibi event durumu, 
            //requestId ise flowRequest tablosundaki onay sırası,veritabanındaki nesne sırası,sorgu ile getirilmeli
            pr.Continue(requestId: 3, 5); //FindRequestId Fonksiyonu Kullanılmalı

        }

    }
}

void CreateSubFlowWithTable()
{
    using (eBAConnection con = CreateServerConnection())
    {
        con.Open();

        eBAForm frm = Document1.ProfileData;

        FormDetailsGridRow formDetailsGridrow = frm.DetailsGrids["tblGozdengecirme"];

        Convert.ToInt32(formDetailsGridrow.Order.ToString());

        //Ana Formumuzun tablosunda kaç tane satır varsa o kadar akış tetiklenecek
        foreach (var row in frm.Tables["tblGozdengecirme"].Rows)
        {

            //Başlatılacak Akışın ismini giriyoruz
            WorkflowProcess mgr = con.WorkflowManager.CreateProcess(process: "EgitimGozdengecirme");

            mgr.Parameters.Add("AnaFormId", Document1.ProfileId.ToString());
            mgr.Parameters.Add("AnaAkisId", id.ToString());    // Ana akış üzerindeki ıd değeri
            mgr.Parameters.Add("AkisBaslatanId", row["ID"].AsString); // Akış tablodaki ekli kullanıcılara gidecek, ıd sütunun dan değeri alıyoruz
            //mgr.Parameters.Add("TabloSatirNo", row.Order.ToString()); // Tablo Satir No yu alt akışa gönderdik

            //mgr.Parameters.Add("flowCount", frm.Tables["tblGozdengecirme"].Rows.Count.ToString());

            mgr.Parameters.Update();

            mgr.Start();

            //For İnsert Database
            AddSubFlow(tetiklenenId: mgr.ProcessId, orderNo: GetOrderID(mgr.ProcessId), Aciklama: "Alt Akış - " + row.Order.ToString());
        }

    }

}

void CreateSubFlowWithDetaylar()
{
    eBAForm frm = new eBAForm(Document2.ProfileId);
    eBAConnection con = CreateServerConnection();

    con.Open();

    FormDetails fd = frm.Details["DT"];
    int a = 0;
    //string rowCount = frm.Tables["tblGozdengecirme"].Rows.Count.ToString();
    foreach (var row in fd.Rows) //frm.Details["DT"].Rows
    {

        var modalForm = row.Form; // Detay Tablo Bir Form gibi Davranıyor
        WorkflowProcess mgr = con.WorkflowManager.CreateProcess(process: "Gorev");

        //mgr.Parameters.Add("AnaFormId", Document1.ProfileId.ToString());
        mgr.Parameters.Add("AnaAkisId", id.ToString());    // Ana akış üzerindeki ıd değeri
        mgr.Parameters.Add("AkisBaslatanId", modalForm.Fields["txtIsYapacakId"].AsString); //Akış tablodaki ekli kullanıcılara gidecek, ıd sütunun dan değeri alıyoruz    
        //mgr.Parameters.Add("AkisBaslatanId", row.Form.Id.ToString()); //Kontrol et

        mgr.Parameters.Update();

        mgr.Start();
        a++;
        //Method For İnsert Database 
        AddSubFlow(tetiklenenId: mgr.ProcessId, orderNo: GetOrderID(mgr.ProcessId), Aciklama: "Alt Akış - " + row.Order.ToString());
    }
    vAltAkisSayisi.ValueAsInteger = a;

}

void CreateSubFlowWithListBox()
{

    eBAForm f = Document1.ProfileData; //veya new new eBAForm(Dokuman1.ProfileId)  şeklinde ulaşıyoruz
    FormList fl = f.Lists["ListBox1"];
    foreach (FormListRow flr in fl.Rows)
    {

        WorkflowProcess mgr = con.WorkflowManager.CreateProcess(process: "YoneticininEkibi");

        mgr.Parameters.Add("AnaFormId", Document1.ProfileId.ToString());
        mgr.Parameters.Add("AnaAkisId", id.ToString());
        mgr.Parameters.Add("AkisBaslatanId", flr.Value);

        mgr.Parameters.Update();

        mgr.Start();

        AddSubFlow(tetiklenenId: mgr.ProcessId, orderNo: GetOrderID(mgr.ProcessId), Aciklama: "Alt Akış - " + flr.Text);
    }
}

/// <summary>
/// Alt Akış Oluşturacağımız zaman FlowSubFlow tablosuna kayıt ekliyoruz
/// </summary>
/// <param name="tetiklenenId"></param>
/// <param name="orderNo"></param>
/// <param name="Aciklama"></param>
void AddSubFlow(int tetiklenenId, int orderNo, string Aciklama)
{
    eBAConnection con = CreateServerConnection();
    con.Open();

    //SubFlow Tablosuna Kayıt atıyoruz, alt akış oluşturduğumuz zaman bu fonksiyonu çağırmamız gerekir
    string sql = string.Format(@"INSERT INTO FLOWSUBFLOWS (PROCESSID,SUBPROCESSID,ORDERNO,DESCRIPTION,RELATIONDATE,RELATIONTYPE) 
                                        VALUES('" + id.ToString() + "','" + tetiklenenId + "','" + orderNo + "','" + Aciklama + "',getdate(),'1')");
    //ORDERNO ana akıstakı en son adımdan bır fazla verebılırsınız hardcoded.
    //relation typei 1 veriniz

    eBADBProvider db = CreateDatabaseProvider();
    SqlConnection SqlCon = (SqlConnection)db.Connection;
    SqlCon.Open();
    try
    {
        SqlCommand com = new SqlCommand(sql, SqlCon);
        com.ExecuteNonQuery();
        com.Dispose();
    }
    finally
    {
        SqlCon.Close();
        con.Close();
    }
}

/// <summary>
/// FlowSubFlow tablosuna kayıt atarken orderId'sini alıyoruz.ana akışın idsini parametre geçiyoruz
/// </summary>
/// <param name="AkisID"></param>
/// <returns></returns>
int GetOrderID(int AkisID) //buraya ana akışın idsini parametre geçiyoruz.
{
    eBADBProvider SqlCon = CreateDatabaseProvider();
    SqlCon.Open();
    try
    {
        string Sql = "Select Max(ORDERNO) AS ORDERNO From FLOWREQUESTS Where ProcessId=" + AkisID;
        SqlDataAdapter da = (SqlDataAdapter)SqlCon.CreateDataAdapter(Sql);
        DataTable dt = new DataTable();
        da.Fill(dt);
        if (dt.Rows.Count > 0)
        {
            return int.Parse(dt.Rows[0]["ORDERNO"].ToString());
        }
        else
        {
            throw new Exception("Sorgu Hiç Satır İçermiyor :\n" + Sql);
        }
    }
    catch (Exception e)
    {
        throw new Exception(e.Message);
    }
    finally
    {
        SqlCon.Close();
    }
}

/// <summary>
/// Üst Akışta Devam Edilecek Request,Process ID sini ve Nesne İsmini biliyoruz, sorgu ile request Id'sini alıyoruz
/// </summary>
/// <param name="processId"></param>
/// <param name="pauserName"></param>
/// <returns></returns>
int findRequestId(int processId, string pauserName)
{
    eBADBProvider SqlCon = CreateDatabaseProvider();
    SqlCon.Open();
    string sqlStr = "SELECT * FROM FLOWREQUESTS " +
                    "WHERE PROCESSID=" + processId + "   AND STATUS=1 " +
                    "AND FLOWOBJECT='" + pauserName + "'";
    SqlDataAdapter da = (SqlDataAdapter)SqlCon.CreateDataAdapter(sqlStr);
    DataTable dt = new DataTable();
    da.Fill(dt);
    if (dt.Rows.Count > 0)
    {
        return int.Parse(dt.Rows[0]["ID"].ToString());
    }
    else
    {
        throw new Exception("No rows were returned from the query. ");
    }
}

void UstAkisDevam()
{

    eBAConnection con = CreateServerConnection();
    con.Open();
    WorkflowManager mg = con.WorkflowManager;
    WorkflowProcess process = mg.GetProcess(Convert.ToInt32(AnaAkisId.Value));
    process.Continue(findRequestId(Convert.ToInt32(AnaAkisId.Value), "AltAkisPauser"), 5); // 5 Approve Durumu -// 6 Red Durumu

}


void attToDmOnAfterAtttach()
{
    eBAConnection eBACon = CreateServerConnection();
    eBACon.Open();

    FileSystem fs = eBACon.FileSystem;
    DMFile form = fs.GetFile("workflow/SozlesmeYonetimi/frmSozlesmeTanim/" + id.ToString() + ".wfd"); //Document1.Path  

    string destPath = "Sözleşme Yönetimi/Temp/" + e.Filename;
    string categoryName = "default";

    try
    {
        //DMFileContent dmc = form.GetAttachments(categoryName).Select(f => f.ContentName == e.Filename); //Dene               		
        DMFile att = fs.CreateFile(destPath);
        att.UploadContentFromByteArray(form.DownloadAttachmentContentBytes(categoryName, e.Filename));  //dmc.ContentName
    }
    catch
    {

    }
    finally
    {
        eBACon.Close();
    }
}

void attToDm2()
{

    eBAConnection con = CreateServerConnection();
    try
    {
        con.Open();
        FileSystem fs = con.FileSystem;
        int formId = Document1.ProfileId;

        DMFile form = fs.GetFile("workflow/SozlesmeYonetimi/frmSozlesmeTanim/" + formId + ".wfd"); //Document1.Path

        string categoryName = "default";  // Attachment Category Name
        string targetFolder = "SozlesmeYonetimi/" + vSozlesmeTipi.Value; // Hangi klasöre taşıyacağız 

        foreach (DMFileContent dmc in form.GetAttachments(categoryName))   // Category ismine göre attachment ları getiriyoruz
        {
            //DMFileContent dmc = form.GetAttachments(categoryName).Select(f => f.ContentName == e.FileName);

            string attName = dmc.ContentName;

            if (!fs.HasFolder(targetFolder)) // Klasör yoksa Oluşturuyoruz
                fs.CreateFolder(targetFolder);

            string fileFullPath = targetFolder + "/" + attName;
            if (!fs.HasFile(fileFullPath))                        // Dosyamız yoksa ekliyoruz  
            {
                DMFile att = fs.CreateFile(fileFullPath);
                att.UploadContentFromByteArray(form.DownloadAttachmentContentBytes(categoryName, attName));
                att.ObjectProperties.Profile = Document1.ProfileId; //Profil Formu Atıyoruz
            }
        }
    }
    finally
    {
        con.Close();
    }

}

/// <summary>
/// DM Kod Versiyon 
/// </summary>
static void AttToDm()
{
    eBAConnection con = CreateServerConnection();
    try
    {
        con.Open();
        FileSystem fs = con.FileSystem;
        int formId = Document1.ProfileId;
        DMFile form = fs.GetFile("workflow/ProjeName/Form/" + formId + ".wfd"); //Document1.Path

        string categoryName = "Sozlesme";  // Attachment Category Name
        string targetFolder = "Kütüphane/Test/" + DateTime.Year.ToString() + "/" + DateTime.Now.Month.ToString("D2"); // Hangi klasöre taşıyacağız,Kütüphane standart 
        foreach (DMFileContent dmc in form.GetAttachments(categoryName))   // Category ismine göre attachment ları getiriyoruz
        {
            DMFileContent dmac = form.GetAttachments(categoryName).Select(f => f.ContentName == e.FileName);
            string attName = dmc.ContentName;
            if (!fs.HasFolder(targetFolder + "/" + categoryName)) // Klasör yoksa Oluşturuyoruz
                fs.CreateFolder(targetFolder + "/" + categoryName);

            string fileFullPath = targetFolder + "/" + attName;
            if (!fs.HasFile(fileFullPath))                        // Dosyamız yoksa ekliyoruz  
            {
                DMFile att = fs.CreateFile(fileFullPath);
                att.UploadContentFromByteArray(form.DownloadAttachmentContentBytes(categoryName, attName));

            }
        }
    }
    finally
    {
        con.Close();
    }

    throw new Exception("0101");
}

void DmToLocal()
{

    //Versiyonsuz İndirme:

    eBAConnection conn = CreateServerConnection();
    string path = @"DENEME/Versiyon/sample.pdf"; //Dm doküman pathi
    try
    {

        conn.Open();
        FileSystem fs = conn.FileSystem;

        string folderPath = @"C:\\TEMP";
        System.IO.Directory.CreateDirectory(folderPath);
        fs.DownloadFileContentToFile(path, folderPath + "\\" + "sample.pdf");

    }
    catch (Exception ex)
    {
        throw new Exception(" An error ocured while copying attachments!\n" + ex.Message);
    }
    finally
    {
        conn.Close();
    }


    // VERSİYONLU İndirme:
    eBAConnection con = CreateServerConnection();
    string path = @"DENEME/Versiyon/sample.pdf?version=1";
    try
    {

        con.Open();
        FileSystem fs = con.FileSystem;

        string folderPath = @"C:\\TEMP";
        System.IO.Directory.CreateDirectory(folderPath);
        fs.DownloadFileContentToFile(path, folderPath + "\\" + "sample.pdf");

    }
    catch (Exception ex)
    {
        throw new Exception(" An error ocured while copying attachments!\n" + ex.Message);
    }
    finally
    {
        con.Close();
    }


}

void AttachToLocal()
{

    /*
    //projeye sag tıklayarak referans dosyaları sunu yazalım 
    % SystemPath %\Common\eBAPI.dll

    //Asagıdaki kodu formun tepesine
                using eBAPI;
                using eBAPI.Connection;
                using eBAPI.DocumentManagement; */

    eBAConnection con = CreateServerConnection();
    try
    {
        con.Open();
        FileSystem fs = con.FileSystem;
        DMFile df = fs.GetWorkflowFile(formid);
        foreach (DMFileContent dc in df.GetAttachments("default"))
        {
            df.DownloadAttachmentContentToFile("default", dc.ContentName, "C:\\eBA\\Attach\\" + dc.ContentName); //Sunucudadaki indirlecek path
        }
    }
    finally
    {
        con.Close();
    }

}

/// <summary>
/// Yeni Versiyon Oluşturup Published Durumunu True Yapma
/// </summary>
static void AttachToDmWithVersionAndPublished()
{
    eBAConnection con = CreateServerConnection();
    try
    {
        con.Open();
        FileSystem fs = con.FileSystem;

        //int formId = AnalizFormu.ProfileId;
        //DMFile form = fs.GetFile("workflow/ProjeName/Form/" + formId + ".wfd");
        DMFile dmFile = fs.GetFile(AnalizFormu.ProfileId);
        //eBAForm frm = AnalizFormu.ProfileData;

        //string s = frm.DocumentPath;//Kontrol Et

        string folderPath = "Sınav Dosyaları/Analiz Formu/Ekler";
        //string filePath = "Sınav Dosyaları/Analiz Formu/Ekler/[" + frm.Fields["txtTalepNo"].Value + "].pdf";
        string categoryName = "default";

        if (!fs.HasFolder(folderPath)) // Klasör yoksa Oluşturuyoruz
            fs.CreateFolder(folderPath);

        foreach (DMFileContent dmc in dmFile.GetAttachments(categoryName))   // Category ismine göre attachment ları getiriyoruz
        {
            string attName = dmc.ContentName;
            byte[] arr = dmFile.DownloadAttachmentContentBytes(categoryName, attName);


            string fileFullPath = folderPath + "/" + attName;

            if (!fs.HasFile(fileFullPath))                        // Dosyamız yoksa ekliyoruz  
            {
                DMCreateFileParameters dcfp = new DMCreateFileParameters();
                DMVersion vrs = new DMVersion(1, 0);
                dcfp.Version = vrs;

                DMFile newFile = fs.CreateFile(fileFullPath, dcfp);
                newFile.UploadContentFromByteArray(arr);
                newFile.SetPublishedVersion(newFile.Version); //Yayınlıyoruz -  Published True olarak geliyor

            }
            else
            {
                DMFile versionFile = fs.CreateFileMajorVersion(fileFullPath); //CreateFileMinorVersion
                versionFile.UploadContentFromByteArray(arr);
                versionFile.SetPublishedVersion(versionFile.Version);
            }
        }
    }
    finally
    {
        con.Close();
    }

}

/// <summary>
/// Üst Formdan Documanın Id değerini Alt formada Aktardıktan Sonra AltFormdaki veri ile Senkronize Ediyoruz
/// </summary>
static void fnSenkronizeForm_Execute()
{

    eBAForm TaskStart = new eBAForm(Convert.ToInt32(TaskStartFormId.Value));
    eBAForm TaskFinish = new eBAForm(GorevTamamlamaForm.ProfileId);

    TaskFinish.Fields["cmbIsYapacakKisi_TEXT"].Value = TaskStart.Fields["cmbIsYapacakKisi_TEXT"].Value;
    TaskFinish.Fields["cmbIsYapacakKisi"].Value = TaskStart.Fields["cmbIsYapacakKisi"].Value;
    TaskFinish.Fields["txtIsTanimi"].Value = TaskStart.Fields["txtIsTanimi"].Value;
    TaskFinish.Fields["txtTahminiCalismaSuresi"].Value = TaskStart.Fields["txtTahminiCalismaSuresi"].Value;
    TaskFinish.Fields["txtTerminTarihi"].Value = TaskStart.Fields["txtTerminTarihi"].Value;
    TaskFinish.Fields["GorevAciklamasi"].Value = TaskStart.Fields["GorevAciklamasi"].Value;

    int FormId = int.MaxValue;
    eBAForm frm = new eBAForm(FormId);
    frm.Fields["Metin1"].AsString = "String Veri"; // Metin Güncelleme
    frm.Fields["Metin2"].AsInteger = 3; // Tam Sayı Güncelleme
    frm.Fields["Metin3"].AsDateTime = new DateTime(2019, 4, 16); // Tarih Güncelleme
    frm.Fields["Secim1"].AsBool = true; // Seçim Güncelleme (Tekli ve Çoklu seçim kutusu)
    frm.Fields["Metin4"].AsDouble = 3.14; // Virgüllü Sayı Güncelleme
    frm.Fields["Liste1_TEXT"].AsString = "String Text Verisi"; // Liste Text değerini Güncelleme
    frm.Fields["Liste1"].AsString = "String Value Verisi"; // Liste Value Değerini Güncelleme
    frm.Fields["Metin5"].Value = Convert.ToDecimal(153.55); // Para Değeri Güncelleme
    frm.Update(); // Güncellenen verileri veritabanına yazma




    TaskFinish.Update();  // verileri Kaydeder
}

static void SenkonizeVariables()
{
    //RadioList verilerine alt akışta ulaşılabilen bir nesne değil
    //Form isimlendirmelerine ve senkronize edilen formun update edilmesine dikkat edilmeli

    eBAForm senkEden = new eBAForm(Convert.ToInt32(AnaFormId.Value));
    eBAForm senkEdilen = Document1.ProfileData;


    eBAConnection con = CreateServerConnection();


    //CheckListSenkronize
    FormCheckList anaFcl = senkEden.CheckLists["CheckListAdı"];
    FormCheckList altFcl = senkEdilen.CheckLists["CheckListAdı"];

    foreach (FormCheckListRow anaFr in anaFcl.Rows)
    {
        altFcl.Rows.Add(anaFr.Value, anaFr.Text);
    }

    //Table Senkronize
    FormTable anaTbl = senkEden.Tables["TableAdı"];
    FormTable altTbl = senkEdilen.Tables["TableAdı"];

    int i = 0;
    foreach (FormTableRow anaRow in anaTbl.Rows)
    {
        i++;
        if (anaRow["txt_EkstraTalep"].AsInteger > 0)
        {
            FormTableRow altRow = altTbl.Rows[i];

            altRow["FIRSTNAME"].AsString = anaRow["FIRSTNAME"].AsString;
            altRow["LASTNAME"].AsString = anaRow["LASTNAME"].AsString;

        }

    }


    //Attachment Senkronize
    try
    {
        con.Open();

        FileSystem fs = con.FileSystem;
        DMFile flAlinacakAttachment = fs.GetFile(senkEden.DocumentPath);
        DMFile flAtilacakAttachment = fs.GetFile(senkEdilen.DocumentPath);
        foreach (DMFileContent content in flAlinacakAttachment.GetAttachments("default"))
        {
            flAtilacakAttachment.UploadAttachmentContentFromStream("default", content.ContentName, flAlinacakAttachment.CreateAttachmentContentDownloadStream("default", content.ContentName));
            flAtilacakAttachment.DeleteAttachmentContent();

            eBAMailAPI.eBAMail eBA = new eBAMail();


        }

    }
    finally
    {
        con.Close();
    }

    //Details senkronize
    try
    {

        FormDetails anaDtl = senkEden.Details["Details ismi"]; //Details ismi
        FormDetails altDtl = senkEdilen.Details["Details ismi"]; //Details ismi

        foreach (var anaRow in anaDtl.Rows)     //Sırayla satırların formunda geziyoruz
        {
            eBAForm anaModalForm = anaRow.Form;

            WorkflowManager mng = con.WorkflowManager;
            WorkflowDocument doc = mng.CreateDocument("GenelMudur", "Form"); //Detayların Bağlı Olduğu Form

            altDtl.Rows.Add(doc.DocumentId);

            eBAForm altModalForm = new eBAForm(doc.DocumentId);
            altModalForm.Fields["cmbOdemeTipi"].AsString = anaModalForm.Fields["cmbOdemeTipi"].AsDateTime;
            altModalForm.Fields["cmbOdemeTipi_TEXT"].AsString = anaModalForm.Fields["cmbOdemeTipi_TEXT"].AsString;
            altModalForm.Fields["txtTutar"].AsString = anaModalForm.Fields["txtTutar"].AsString;

            altModalForm.Update();

        }
    }
    finally
    {
        con.Close();

    }


    //Details Grid Senkronize
    FormDetailsGrid AnaDg = senkEden.DetailsGrids["Details Grid İsmi"];
    FormDetailsGrid altDg = senkEdilen.DetailsGrids["Details Grid İsmi"];

    foreach (FormDetailsGridRow anaDgr in AnaDg.Rows)
    {
        FormDetailsGridRow alt_row = altDg.Rows.Add();
        alt_row["txtInt"].Value = anaDgr["txtInt"].ToString();
        alt_row["txtFloat"].AsString = anaDgr["txtFloat"].ToString();
        alt_row["txtDate"].Value = anaDgr["txtDate"].ToString();
        alt_row["txtText"].Value = anaDgr["txtText"].ToString();
    }

    //ListBox Senkronize
    FormList AnaLb = senkEden.Lists["ListBox İsmi"];
    FormList AltLb = senkEdilen.Lists["ListBox İsmi"];

    foreach (FormListRow anaFlr in AnaLb.Rows)
    {
        AltLb.Rows.Add(anaFlr.Value, anaFlr.Text);
    }

    //Related Document Senkronize
    try
    {
        con.Open();
        FileSystem fs = con.FileSystem;
        DMFile flAlinacakRelations = fs.GetFile(senkEden.DocumentPath);
        DMFile flAtilacakAttachment = fs.GetFile(senkEdilen.DocumentPath);

        fs.AddFileRelation();

        foreach (DMFileRelation rel in flAlinacakRelations.GetRelations())
        {
            //if(rel.Category=="default") //formda birden fazla relations varsa category kontrol ediyoruz.
            //String path = rel.Path; // böyle pathi alıp aşağıda DMFile classına gidip streamini alabilirsiniz
            //DMFile doc = fs.GetFile(path);
            //Stream str = doc.Download(); 
            //flAtilacakAttachment.UploadAttachmentContentFromStream("default", doc.Content.Name, str);
            flAtilacakAttachment.AddRelation(rel.Path, "default");
        }

    }
    finally
    {
        con.Close();
    }

    senkEdilen.Update(); //Update Yapılmazsa Form Senkronize Olmaz
}

void DetailsGridAddingValidation()
{
    if (args.isnew)
    {
        if (Detaylar1.Data.Rows.Count == 1)
        {
            args.allow = false;
        }
    }
}

void RelationEkle()
{
    eBAConnection con = CreateServerConnection();
    con.Open();

    FileSystem fs = con.FileSystem;
    DMFile fl = fs.GetWorkflowFile(id); //relations alacağımız formu alıyoruz.
    DMFile fl2 = fs.GetWorkflowFile(id); //bulunduğumuz formu alıyoruz.

    foreach (DMFileRelation rel in fl.GetRelations())
    {
        if (rel.Category == "category1") //formda birden fazla relations varsa category kontrol ediyoruz.
        {
            fl2.AddRelation(rel.Path, "category2"); // baglantı kuracagımız full path ve nesne kategorisi
            con.FileSystem.SetFileRelationDescription("Workflow/D19_4/Form/" + id + ".wfd", rel.Path, "category2", rel.Description); //SetFileRelationDescription(form.Path, fl.Path,"default", fl.Description);
        }
    }

    con.Close();

}

void DateTime()
{
    eBADateTimeBox dateTimeBox = new eBADateTimeBox();

    dateTimeBox.RestrictWeekends();

    DateTime dt = DateTime.MinValue;
    DateTime dt2 = dt.AddDays(1);


    if (dt.CompareTo(dt2) < 0) //='dan küçükse dt2den küçüktür
    {


    }
}

void DetayTablo()
{

    eBAForm eventForm = new eBAForm(CurrentEventFormId); // eventFormId
    eBAForm prmForm = new eBAForm(Document2.ProfileId);  // parametrikFormId

    FormDetailsGridRow newRow = prmForm.DetailsGrids["dtgEgitimBilgileri"].Rows.Add();

    newRow["txtEgitimKonusu"].AsString = eventForm.Fields["txtEgitimKonusu"].AsString;
    newRow["cmbEgitmen_TEXT"].AsString = eventForm.Fields["dcEgitmen"].AsString;
    newRow["cmbEgitmen_TEXT"].AsString = eventForm.Fields["dcEgitmenId"].AsString;
    newRow["txtEgitimBaslangici"].AsString = eventForm.Fields["txtEgitimBaslangici"].AsString;
    newRow["txtEgitimBitisi"].AsString = eventForm.Fields["txtEgitimBitisi"].AsString;


    //frm.DetailsGrids["DetayTabloAdı"].Rows[int SatırNo]["KolonAdı"].AsString = "String Veri";  
    //Tüm Satırları döngü ile işleyip veriyi güncelleme 


    prmForm.Update(); // Güncellenen verileri veritabanına yazma 
}

#endregion