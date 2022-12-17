using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.IO.Compression;

namespace Server.Forms
{
    public partial class FormCertificate : Form
    {
        public FormCertificate()
        {
            InitializeComponent();
        }

        private void FormCertificate_Load(object sender, EventArgs e)
        {
            try
            {
                string backup = Application.StartupPath + "\\BackupCertificate.zip";
                if (File.Exists(backup))
                {
                    MessageBox.Show(this, "找到zip备份，正在解压缩（BackupCertificate.zip）", "证书备份", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ZipFile.ExtractToDirectory(backup, Application.StartupPath);
                    Settings.ServerCertificate = new X509Certificate2(Settings.CertificatePath);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Certificate", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textBox1.Text)) return;

                button1.Text = "请稍等...";
                button1.Enabled = false;
                textBox1.Enabled = false;
                await Task.Run(() =>
                {
                    try
                    {
                        string backup = Application.StartupPath + "\\BackupCertificate.zip";
                        Settings.ServerCertificate = Helper.CreateCertificate.CreateCertificateAuthority(textBox1.Text, 4096);
                        File.WriteAllBytes(Settings.CertificatePath, Settings.ServerCertificate.Export(X509ContentType.Pkcs12));

                        using (ZipArchive archive = ZipFile.Open(backup, ZipArchiveMode.Create))
                        {
                            archive.CreateEntryFromFile(Settings.CertificatePath, Path.GetFileName(Settings.CertificatePath));
                        }
                        Program.form1.listView1.BeginInvoke((MethodInvoker)(() =>
                        {
                            MessageBox.Show(this, @"
[!] 如果要升级到AsyncRAT的新版本,则需要复制“ServerCertificate.p12”.

[!] 如果丢失\删除“ServerCertificate”,p12证书您将无法控制您的被控端,您将失去所有被控端.", "证书", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Close();
                        }));
                    }
                    catch (Exception ex)
                    {
                        Program.form1.listView1.BeginInvoke((MethodInvoker)(() =>
                        {
                            MessageBox.Show(this, ex.Message, "Certificate", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            button1.Text = "确定";
                            button1.Enabled = true;
                            textBox1.Enabled = true;
                        }));
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Certificate", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                button1.Text = "确定";
                button1.Enabled = true;
            }
        }

    }
}
