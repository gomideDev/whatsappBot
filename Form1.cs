using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MMLib;
using System.Threading.Tasks;
using SeleniumExtras.WaitHelpers;
using System.IO;

namespace whatsappBot
{
    public partial class Form1 : Form
    {
        private int qtdMsg;
        private int qtdMsgEnviadas = 0;
        private int tempoPausa;
        private string message;
        private List<string> numeros = null;
        private int qtdMsgPausa = 0;
        private int contadorMsg = 0;
        private DateTime dataHoraInicio;

        public Form1()
        {
            InitializeComponent();
            this.Icon = new System.Drawing.Icon(@".\check.ico");
        }
        private IWebDriver driver;
        private WebDriverWait wait;
        private void BtnExit_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Deseja realmente sair?", "Sair", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                Application.Exit();
            }
        }

        private void BtnMinimiza_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {;
            iniciar();
        }


        private IWebElement AguardarElementoVisivel(By locator, IWebDriver driver, int maxSeconds)
        {
            try
            {
                return new WebDriverWait(driver, TimeSpan.FromSeconds(maxSeconds))
                    .Until(ExpectedConditions.ElementExists(locator));
            }
            catch (Exception)
            {
                EnviarMenssagem(driver, wait);
            }
            return null;
        }

        private void iniciar()
        {
            try
            {
                timer1.Interval = 60000;
                timer1.Start();
                ConfiguraVariaveis();
                BloqueiaForm();

                //Configura e inicia o driver do Chrome
                ChromeOptions options = new ChromeOptions();
                options.AddArguments("--disable-infobars");
                driver = new ChromeDriver(options);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(120);
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));
                EnviarMenssagem(driver, wait);
            }
            catch (FormatException)
            {
                MessageBox.Show("Caracteres inválidos, favor verificar e tentar novamente", "Caracteres invalidos", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Arquivo de dados não definido", "Telefones não encontrados", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (WebDriverTimeoutException)
            {
                driver.Quit();
                iniciar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
}

        private void DesbloqueiaForm()
        {
            this.txtTempoPausa.Enabled = true;
            this.txtMsg.Enabled = true;
            this.txtQtdMsg.Enabled = true;
            this.btnStart.Enabled = true;
        }

        private void EnviarMenssagem(IWebDriver driver, WebDriverWait wait)
        {
            try
            {
                
                if (this.qtdMsgEnviadas < this.numeros.Count && this.numeros != null && this.qtdMsgEnviadas <= this.qtdMsg)
                {
                    if(this.contadorMsg == this.qtdMsgPausa)
                    {
                        TempoPausa();
                    }
                    string numeroParaEnviar = this.numeros[this.qtdMsgEnviadas];
                    CarregaApi(numeroParaEnviar, driver);

                    Thread.Sleep(5000);

                    IWebElement txtSend = AguardarElementoVisivel(By.XPath("//*[@id='main']/footer/div[1]/div[2]/div/div[2]"), driver, 120);
                    txtSend.SendKeys(this.message + OpenQA.Selenium.Keys.Enter);

                    Thread.Sleep(10000);
                    this.qtdMsgEnviadas++;
                    this.contadorMsg++;
                    EnviarMenssagem(driver, wait);

                       
                }
                else
                {
                    Fechar();
                    
                }
            }
            catch (Exception )
            {
                Fechar();
            }
        }

        private void Fechar()
        {
            this.dataHoraInicio = DateTime.Now;
            driver.Quit();
            string nomeArquivo = @".\whatsappBot-Log" + DateTime.Now.ToString("yyyymmddHHmm") + ".txt";
            StreamWriter stream = new StreamWriter(nomeArquivo);
            stream.WriteLine("whatsappBot iniciado em: " + this.dataHoraInicio + " e finalizado em: " + DateTime.Now +
                "\n Mensagens enviadas para: " + this.qtdMsgEnviadas + " Contatos");
            stream.Close();
            this.DesbloqueiaForm();
            Application.Exit();
        }

        private void TempoPausa()
        {
            Thread.Sleep(this.tempoPausa *60000);
            this.contadorMsg = 0;
            EnviarMenssagem(driver, wait);
        }

        private void CarregaApi(string telefone ,IWebDriver driver)
        {
            driver.Navigate().GoToUrl("https://api.whatsapp.com/send?phone="+telefone+"&text=");
            IWait<IWebDriver> esperar = new WebDriverWait(driver, TimeSpan.FromSeconds(120));
            esperar.Until(x => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            Thread.Sleep(5000);
        }

        private void ObtemNumeros(string path)
        {
            try
            {
                this.numeros = System.IO.File.ReadAllLines(path).ToList();
                this.lblQtdLinhas.Text = numeros.Count.ToString() +" Contatos";

            }catch(NullReferenceException)
            {
                throw new Exception("Caminho do arquivo de dados não especificado, favor selecione um arquivo de dados valido!");
                    
            }catch(Exception ex)
            {
                throw new Exception("Erro: " + ex.Message);
            }
        }

        private void ConfiguraVariaveis()
        {
            try
            {
                this.dataHoraInicio = DateTime.Now;
                this.qtdMsg = Convert.ToInt32(txtQtdMsg.Text);
                this.tempoPausa = Convert.ToInt32(txtTempoPausa.Text);
                this.message = txtMsg.Text;
                this.qtdMsgPausa = Convert.ToInt32(txtQtdMsgPausa.Text);
                if(this.numeros == null)
                {
                    throw new NullReferenceException("Arquivo de dados não definido");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void BloqueiaForm()
        {
            this.txtTempoPausa.Enabled = false;
            this.txtMsg.Enabled = false;
            this.txtQtdMsg.Enabled = false;
            this.btnStart.Enabled = true;
            //this.WindowState = FormWindowState.Minimized;
        }

        private void BtnProcurar_Click(object sender, EventArgs e)
        {
            PocurarArquivoDados();
        }

        private void PocurarArquivoDados()
        {
            try
            {
                string path;
                OpenFileDialog dialog = new OpenFileDialog();

                dialog.Multiselect = false;
                dialog.Title = "Selecionar arquivo de dados";
                dialog.Filter = "txt files (*.txt)|*.txt";
                

                if(dialog.ShowDialog() == DialogResult.OK)
                {
                    path = dialog.FileName;
                    this.ObtemNumeros(path); 
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            PocurarMensagem();
        }

        private void PocurarMensagem()
        {
            try
            {
                
                string[] mensagem = null;
                string path;
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "txt files (*.txt)|*.txt";
                dialog.Multiselect = false;
                dialog.Title = "Procurar Mensagem";

                if(dialog.ShowDialog() == DialogResult.OK)
                {
                    path = dialog.FileName;
                    mensagem = System.IO.File.ReadAllLines(path);
                    this.txtMsg.Lines = mensagem;
                    this.lblPathMsg.Text = path;
                }
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CarregaValoresDefalt();
        }

        private void CarregaValoresDefalt()
        {
            this.txtTempoPausa.Text = "1";
            this.txtQtdMsg.Text = "50";
            this.txtQtdMsgPausa.Text = "20";
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            EnviarMenssagem(driver, wait);
        }
    }
}
