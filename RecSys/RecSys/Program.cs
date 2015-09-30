using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecSys
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    namespace Recsys
    {

        public class Test
        {

            public Test()
            {
            }

            static void Main()
            {
                Test.setTestTrainingSet();
            }

            static void setTestTrainingSet()
            {
                string nomeDaPasta = @"C:\Users\Bruno\Downloads\kfolds\kfold-pasta-";
                string diretorio = @"C:\Users\Bruno\Downloads\kfolds";
                string caminhoArquivoDestinoTest = @"C:\Users\Bruno\Downloads\kfolds\test.txt";
                string caminhoArquivoDestinotreino = @"C:\Users\Bruno\Downloads\kfolds\training.txt";
                const string destinationFileNamepasta = @"C:\Users\Bruno\Downloads\kfolds\kfold-pasta-{0}";
                DirectoryInfo dir = new DirectoryInfo(@"C:\Users\Bruno\Downloads\kfolds");


                double tam = int.Parse(Console.ReadLine()); ;
                double tamTestSet = int.Parse(Console.ReadLine()); ;
                double tamTrainingSet = tam * (tamTestSet / 100);
                Math.Round(tamTrainingSet);
                double percentTrainingSet = tam - tamTrainingSet;
                var fileCounter = 0;
                HashSet<int> totalSet = new HashSet<int>();

                // preenche o total set e cria as pasta dos k folds
                for (int i = 0; i < tam; i++)
                {
                    totalSet.Add(i);

                    var destino = Directory.CreateDirectory(string.Format(destinationFileNamepasta, fileCounter + 1));

                    fileCounter++;
                }

                Random random = new Random();
                HashSet<int> testSet = new HashSet<int>();

                List<int> teste = new List<int>();
                List<int> treino = new List<int>();


                int num = 1;
                for (int i = 0; i < tam; i++, num++)
                {
                    do
                    {
                        int index = totalSet.ToList()[random.Next(0, totalSet.Count)];
                        testSet.Add(index);
                        totalSet.Remove(index);
                    } while (testSet.Count != tamTrainingSet);



                    Console.Write(" agora o training ");
                    foreach (int training in totalSet)
                    {

                        Console.Write(" " + training);
                    }
                    treino = totalSet.ToList();

                    Console.Write(" agora o test ");
                    foreach (int test in testSet)
                    {

                        Console.Write(" " + test);

                        totalSet.Add(test);
                    }
                    teste = testSet.ToList();
                    testSet.Clear();



                    // gerar test  20%

                    // se  i for maior do que 2 não precisa gerar mais test pq teste sao 20% logo seriam apenas um loop de 2


                    String[] listaDeArquivos = Directory.GetFiles(diretorio);

                    if (listaDeArquivos.Length > 0)
                    {

                        int k = 0;


                        FileStream arquivoDestino = File.Open(caminhoArquivoDestinoTest, FileMode.OpenOrCreate);
                        arquivoDestino.Close();

                        List<String> linhasDestino = new List<string>();
                        int arquivoselecionado;


                        //fileCounter = 0;

                        for (k = 0; k < teste.Count; k++)
                        {


                            arquivoselecionado = teste[k];


                            linhasDestino.AddRange(File.ReadAllLines(listaDeArquivos[arquivoselecionado]));


                            File.WriteAllLines(caminhoArquivoDestinoTest, linhasDestino.ToArray());

                        }
                        string destinoTest = nomeDaPasta + num + "\\test.txt";
                        foreach (FileInfo f in dir.GetFiles("test.txt"))
                            File.Move(caminhoArquivoDestinoTest, destinoTest);

                    }



                    // gerar treino 80%
                    String[] listaDeArquivostreino = Directory.GetFiles(diretorio);

                    if (listaDeArquivostreino.Length > 0)
                    {

                        int k = 0;


                        FileStream arquivoDestino = File.Open(caminhoArquivoDestinotreino, FileMode.OpenOrCreate);
                        arquivoDestino.Close();

                        List<String> linhasDestino = new List<string>();
                        int arquivoselecionado;

                        //var fileCounter = 0;


                        for (k = 0; k < treino.Count; k++)
                        {


                            arquivoselecionado = treino[k];


                            linhasDestino.AddRange(File.ReadAllLines(listaDeArquivostreino[arquivoselecionado]));


                            File.WriteAllLines(caminhoArquivoDestinotreino, linhasDestino.ToArray());


                        }
                        string destinoTraining = nomeDaPasta + num + "\\training.txt";
                        foreach (FileInfo f in dir.GetFiles("training.txt"))
                            File.Move(caminhoArquivoDestinotreino, destinoTraining);

                    }

                }

            }
        }
    }
}
