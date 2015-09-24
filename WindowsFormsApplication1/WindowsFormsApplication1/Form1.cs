using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


/*  cria uma classe de pre processamento vai receber 3 parametros  , o arquivo principal , numero de kfolds e a porcentagem de
treino e test , vai gerar os arquivos com os k fold , depois vai ler dos k fold e gerar para cada k fold um training e test
com  a quantidade determinada pelo usuiario  , training e 80% o test e 20% (valor a ser definido pelo usuario), 
traning partial vai ser opcional com 20% a menos que o training  o test com os msm 20% do test  de forma aleatoria
gerar uma matriz de saida mostrando o arquivos gerados training e test e os k fold se possível*
ex se for gerado 10 arquivos , os arquivos 1-8 serão o training e o 9-10 serão o test é necessario fazer concatenar esses arquivos 
para ser gerados os training e test 
*/

 // 

namespace WindowsFormsApplication1
{

    public partial class Form1 : Form
    {
        private string strPathFile = @"C:\Users\smart\Downloads\ArquivoExemplo.dat_saida";
        const string destinationFileName = @"C:\Users\smart\Downloads\kfolds\Kfold-parte-{0}.txt";
        const string destinationFileNamepasta = @"C:\Users\smart\Downloads\kfolds\Kfold-pasta-{0}";
        public double contador3;
        public int linhasporarquivo = 0;
        public Boolean cb_marcado;
        private static int contador1;
        public int contadorporcento = 0;
        public int flag = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            Criar();
        }


        private void Criar()

        {

            try

            {

                //Usarei a cláusula using como boas práticas de programação em todos os métodos

                //Instancio a classe FileStream, uso a classe File e o método Create para criar o

                //arquivo passando como parâmetro a variável strPathFile, que contém o arquivo

                using (FileStream fs = File.Create(strPathFile))

                {

                    //Crio outro using, dentro dele instancio o StreamWriter (classe para gravar os dados)

                    //que recebe como parâmetro a variável fs, referente ao FileStream criado anteriormente

                    using (StreamWriter sw = new StreamWriter(fs))

                    {

                        //Uso o método Write para escrever algo em nosso arquivo texto

                        sw.Write("Texto adicionado ao exemplo!");

                    }

                }

            }

            catch (Exception ex)

            {

                MessageBox.Show(ex.Message);

            }



            //Se tudo ocorrer bem, exibo a mensagem ao usuário.

            MessageBox.Show("Arquivo criado com sucesso!!!");

        }

        private void btnAbrir_Click(object sender, EventArgs e)
        {
            Abrir();
        }

        private void Abrir()

        {

            try

            {

                //Verifico se o arquivo que desejo abrir existe e passo como parâmetro a respectiva variável

                if (File.Exists(strPathFile))

                {

                    //Se existir "starto" um processo do sistema para abrir o arquivo e, sem precisar

                    //passar ao processo o aplicativo a ser aberto, ele abre automaticamente o Notepad

                    System.Diagnostics.Process.Start(strPathFile);


                }

                else

                {

                    //Se não existir exibo a mensagem

                    MessageBox.Show("Arquivo não encontrado!");

                }

            }

            catch (Exception ex)

            {

                MessageBox.Show(ex.Message);

            }

        }

        private void btnConcatenar_Click(object sender, EventArgs e)
        {
            Concatenar();
        }
        private void Concatenar()

        {

            try

            {

                //Verifico se o arquivo que desejo abrir existe e passo como parâmetro a respectiva variável

                if (File.Exists(strPathFile))

                {

                    //Crio um using, dentro dele instancio o StreamWriter, uso a classe File e o método

                    //AppendText para concatenar o texto, passando como parâmetro a variável strPathFile

                    using (StreamWriter sw = File.AppendText(strPathFile))

                    {

                        //Uso o método Write para escrever o arquivo que será adicionado no arquivo texto


                        string diretorio = @"C:\Users\smart\Downloads\kfolds";

                        String[] listaDeArquivos = Directory.GetFiles(diretorio);

                        if (listaDeArquivos.Length > 0)
                        {
                            string caminhoArquivoDestino = @"C:\Users\smart\Downloads\kfolds\saida.txt";

                            FileStream arquivoDestino = File.Open(caminhoArquivoDestino, FileMode.OpenOrCreate);
                            arquivoDestino.Close();

                            List<String> linhasDestino = new List<string>();

                            foreach (String caminhoArquivo in listaDeArquivos)
                            {
                                linhasDestino.AddRange(File.ReadAllLines(caminhoArquivo));
                            }

                            File.WriteAllLines(caminhoArquivoDestino, linhasDestino.ToArray());
                        }


                        //  sw.Write("\r\nTexto adicionado ao final do arquivo1");



                    }



                    //Exibo a mensagem que o arquivo foi atualizado

                    MessageBox.Show("Arquivo atualizado!");

                }

                else

                {

                    //Se não existir exibo a mensagem

                    MessageBox.Show("Arquivo não encontrado!");

                }

            }

            catch (Exception ex)

            {

                MessageBox.Show(ex.Message);

            }

        }

        private void btnAlterar_Click(object sender, EventArgs e)
        {
            Alterar();
        }
        private void Alterar()

        {

            try

            {

                //Verifico se o arquivo que desejo abrir existe e passo como parâmetro a variável respectiva

                if (File.Exists(strPathFile))

                {

                    //Instancio o FileStream passando como parâmetro a variável padrão, o FileMode que será

                    //o modo Open e o FileAccess, que será Read(somente leitura). Este método é diferente dos

                    //demais: primeiro irei abrir o arquivo, depois criar um FileStream temporário que irá

                    //armazenar os novos dados e depois criarei outro FileStream para fazer a junção dos dois

                    using (FileStream fs = new FileStream(strPathFile, FileMode.Open, FileAccess.Read))

                    {

                        //Aqui instancio o StreamReader passando como parâmetro o FileStream criado acima.

                        //Uso o StreamReader já que faço 1º a leitura do arquivo. Irei percorrer o arquivo e

                        //quando encontrar uma string qualquer farei a alteração por outra string qualquer

                        using (StreamReader sr = new StreamReader(fs))

                        {

                            //Crio o FileStream temporário onde irei gravar as informações

                            using (FileStream fsTmp = new FileStream(strPathFile + ".tmp",

                                                       FileMode.Create, FileAccess.Write))

                            {

                                //Instancio o StreamWriter para escrever os dados no arquivo temporário,

                                //passando como parâmetro a variável fsTmp, referente ao FileStream criado

                                using (StreamWriter sw = new StreamWriter(fsTmp))

                                {

                                    if (flag != 1)
                                    {
                                        int contador = 0;

                                        string strlinha = null;
                                        while ((strlinha = sr.ReadLine()) != null)
                                        {
                                            contador++;

                                        }
                                        flag = 1;
                                        contador1 = contador;

                                    }

                                    int valorNum1 = int.Parse(textBox1.Text);
                                    linhasporarquivo = valorNum1;

                                    linhasporarquivo = contador1 / linhasporarquivo;

                                    using (var sourcefile = new StreamReader(strPathFile))
                                    {

                                        var fileCounter = 0;
                                        var destinationFile = new StreamWriter(string.Format(destinationFileName, fileCounter + 1));
                                      //var destino=  Directory.CreateDirectory(string.Format(destinationFileNamepasta, fileCounter + 1));
                                        
                                        try
                                        {
                                            var lineCounter = 0;
                                            string line;
                                            while ((line = sr.ReadLine()) != null)
                                            {
                                                if (lineCounter >= linhasporarquivo)
                                                {
                                                    //sim.. hora de mudar de arquivo
                                                    lineCounter = 0;
                                                    fileCounter++;
                                                    destinationFile.Dispose();
                                            
                                                    destinationFile = new StreamWriter(string.Format(destinationFileName, fileCounter + 1));
                                                  //   destino = Directory.CreateDirectory(string.Format(destinationFileNamepasta, fileCounter + 1));

                                                }
                                                destinationFile.WriteLine(line);
                                                lineCounter++;


                                            }
                                        }
                                        finally
                                        {
                                            destinationFile.Dispose();
                                        }
                                    }




                                }
                            }
                        }
                    }




                    //Ao final excluo o arquivo anterior e movo o temporário no lugar do original

                    //Dessa forma não perco os dados de modificação de meu arquivo

                    File.Delete(strPathFile);



                    //No método Move passo o arquivo de origem, o temporário, e o de destino, o original

                    File.Move(strPathFile + ".tmp", strPathFile);



                    //Exibo a mensagem ao usuário

                    MessageBox.Show("Arquivo alterado com sucesso!");


                }

                else

                {

                    //Se não existir exibo a mensagem

                    MessageBox.Show("Arquivo não encontrado!");

                }

            }

            catch (Exception ex)

            {

                MessageBox.Show(ex.Message);

            }

        }

        private void btnExcluir_Click(object sender, EventArgs e)
        {
            Excluir();
        }
        private void Excluir()

        {

            try

            {

                //Verifico se o arquivo que desejo abrir existe e passo como parâmetro a variável respectiva

                if (File.Exists(strPathFile))

                {

                    //Se existir chamo o método Delete da classe File para apagá-lo e exibo uma msg ao usuário

                    File.Delete(strPathFile);

                    MessageBox.Show("Arquivo excluído com sucesso!");

                }

                else

                {

                    //Se não existir exibo a mensagem

                    MessageBox.Show("Arquivo não encontrado!");

                }

            }

            catch (Exception ex)

            {

                MessageBox.Show(ex.Message);

            }

        }

        private bool ExisteNumero(List<int> lista, int numero)
        {
            for (int i = 0; i < lista.Count; i++)
            {
                if (lista[i] == numero)
                    return true;
            }

            return false;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {

                cb_marcado = true;

            }
            else
            {
                cb_marcado = false;
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //  int valorNum1 = int.Parse(textBox1.Text);
            //linhasporarquivo = valorNum1;
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            gerartraining();
        }
        private void gerartraining()

        {

            try

            {

                //Verifico se o arquivo que desejo abrir existe e passo como parâmetro a respectiva variável

                if (File.Exists(strPathFile))

                {

                    //Crio um using, dentro dele instancio o StreamWriter, uso a classe File e o método

                    //AppendText para concatenar o texto, passando como parâmetro a variável strPathFile

                    using (StreamWriter sw = File.AppendText(strPathFile))

                    {

                        //Uso o método Write para escrever o arquivo que será adicionado no arquivo texto

                        const string destinationFileName1 = @"C:\smart\smart\Downloads\kfolds\training-parte-{0}.txt";
                        string diretorio = @"C:\Users\smart\Downloads\kfolds";

                        int quantidadearquivos = int.Parse(textBox1.Text);
                        String[] listaDeArquivos = Directory.GetFiles(diretorio);


                        if (listaDeArquivos.Length > 0)
                        {

                            //  1 2 3 4 5 6 7 8 9 10
                            string caminhoArquivoDestino = @"C:\Users\smart\Downloads\kfolds\training.txt";
                            int j, k;


                            FileStream arquivoDestino = File.Open(caminhoArquivoDestino, FileMode.OpenOrCreate);
                            arquivoDestino.Close();

                            List<String> linhasDestino = new List<string>();
                            String caminhoArquivo;

                            int quantidadekfold = int.Parse(textBox1.Text);


                            double oitentaporcento = quantidadekfold * 0.8;
                            
                            for (int i = 0; i < oitentaporcento; i++) { 
                                    for (j = i; j < oitentaporcento; j++)
                                    {

                                    using (var sourcefile = new StreamReader(strPathFile))
                                    {

                                        var fileCounter = 0;
                                        var destinationFile = new StreamWriter(string.Format(destinationFileName1, fileCounter + 1));
                                        try
                                        {
                                            var lineCounter = 0;
                                            string line;
                                          
                                                if (lineCounter >= linhasporarquivo)
                                                {
                                                    //sim.. hora de mudar de arquivo
                                                    lineCounter = 0;
                                                    fileCounter++;
                                                    destinationFile.Dispose();
                                                    destinationFile = new StreamWriter(string.Format(destinationFileName1, fileCounter + 1));
                                                }
                                            caminhoArquivo = listaDeArquivos[j];

                                            linhasDestino.AddRange(File.ReadAllLines(caminhoArquivo));


                                            File.WriteAllLines(caminhoArquivoDestino, linhasDestino.ToArray());
                                            lineCounter++;


                                            
                                        }
                                        finally
                                        {
                                            destinationFile.Dispose();
                                        }
                                    }

                                    

                                    }

                        }

                           
                        }
                        
                         // gerar test 20%

                        if (listaDeArquivos.Length > 0)
                        {

                            //  1 2 3 4 5 6 7 8 9 10
                            string caminhoArquivoDestino = @"C:\Users\smart\Downloads\kfolds\test.txt";
                            int j, k;


                            FileStream arquivoDestino = File.Open(caminhoArquivoDestino, FileMode.OpenOrCreate);
                            arquivoDestino.Close();

                            List<String> linhasDestino = new List<string>();
                            String caminhoArquivo;

                            int quantidadekfold = int.Parse(textBox1.Text);
                         

                            double vinteporcento = quantidadekfold * 0.2;
                   
                            for (j= quantidadekfold - Convert.ToInt32(vinteporcento); j < quantidadekfold; j++)
                            {



                                caminhoArquivo = listaDeArquivos[j];

                                linhasDestino.AddRange(File.ReadAllLines(caminhoArquivo));


                                File.WriteAllLines(caminhoArquivoDestino, linhasDestino.ToArray());

                            }




                        }




                    }



                    //Exibo a mensagem que o arquivo foi atualizado

                    MessageBox.Show("Arquivo atualizado!");

                }

                else

                {

                    //Se não existir exibo a mensagem

                    MessageBox.Show("Arquivo não encontrado!");

                }

            }

            catch (Exception ex)

            {

                MessageBox.Show(ex.Message);

            }
        }
    }
}
    

            

   