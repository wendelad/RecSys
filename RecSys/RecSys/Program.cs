﻿using System;
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
    using System.Text;
    using System.Threading.Tasks;

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

                string diretorio = @"C:\Users\smart\Downloads\kfolds";
                string caminhoArquivoDestino = @"C:\Users\smart\Downloads\kfolds\test.txt";
                string caminhoArquivoDestinotreino = @"C:\Users\smart\Downloads\kfolds\training.txt";
                const string destinationFileNamepasta = @"C:\Users\smart\Downloads\kfolds\kfold-pasta-{0}";
                // const string destinationFileNamepastatreino = @"C:\Users\smart\Downloads\kfolds\treino-pasta-{0}";
                double tam = 10;
                double tamTestSet = 0.2;
                double tamTrainingSet = tam * tamTestSet;
                //Console.Write("testset " + tamTestSet);
                Math.Round(tamTrainingSet);


                double percentTestSet = Math.Round(tam * (tamTestSet / 100));
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
                for (int i = 0; i < 10; i++, num++)
                {
                    do
                    {
                        int index = totalSet.ToList()[random.Next(0, 8)];
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


                        FileStream arquivoDestino = File.Open(@"C:\Users\smart\Downloads\kfolds\test" + num + ".txt", FileMode.OpenOrCreate);
                        arquivoDestino.Close();

                        List<String> linhasDestino = new List<string>();
                        int arquivoselecionado;


                        //fileCounter = 0;

                        for (k = 0; k < teste.Count; k++)
                        {


                            arquivoselecionado = teste[k];


                            linhasDestino.AddRange(File.ReadAllLines(listaDeArquivos[arquivoselecionado]));


                            File.WriteAllLines(@"C:\Users\smart\Downloads\kfolds\test" + num + ".txt", linhasDestino.ToArray());

                        }
                    }



                    // gerar treino 80%
                    String[] listaDeArquivostreino = Directory.GetFiles(diretorio);

                    if (listaDeArquivostreino.Length > 0)
                    {

                        int k = 0;


                        FileStream arquivoDestino = File.Open(@"C:\Users\smart\Downloads\kfolds\training" + num + ".txt", FileMode.OpenOrCreate);
                        arquivoDestino.Close();

                        List<String> linhasDestino = new List<string>();
                        int arquivoselecionado;

                        //var fileCounter = 0;


                        for (k = 0; k < treino.Count; k++)
                        {


                            arquivoselecionado = treino[k];


                            linhasDestino.AddRange(File.ReadAllLines(listaDeArquivostreino[arquivoselecionado]));


                            File.WriteAllLines(@"C:\Users\smart\Downloads\kfolds\training" + num + ".txt", linhasDestino.ToArray());


                        }
                    }
                    DirectoryInfo dir = new DirectoryInfo(@"C:\Users\smart\Downloads\kfolds");
                    string destinoTest = @"C:\Users\smart\Downloads\kfolds\kfold-pasta-"+num+"\\test" + num + ".txt";
                    string destinoTraining = @"C:\Users\smart\Downloads\kfolds\kfold-pasta-" + num + "\\training" + num + ".txt";
                    foreach (FileInfo f in dir.GetFiles("test" + num + ".txt"))
                        File.Move(@"C:\Users\smart\Downloads\kfolds\test" + num + ".txt", destinoTest);
                    foreach (FileInfo f in dir.GetFiles("training" + num + ".txt"))
                        File.Move(@"C:\Users\smart\Downloads\kfolds\training" + num + ".txt", destinoTraining);
                    

                }



            }
        }



    }
}
