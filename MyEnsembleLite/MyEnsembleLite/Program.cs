using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMediaLite;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Eval;
using MyMediaLite.IO;
using MyMediaLite.ItemRecommendation;
using Mono.Options;
using MyMediaLite.Eval.Measures;
using btl.generic;
using GAF;
using GAF.Operators;


namespace MyEnsembleLite
{
	class Program
	{
		//Recommender
		protected List<IRecommender> recommenders = new List<IRecommender>();
		protected string recommender_options = string.Empty;

		// data
		List<IPosOnlyFeedback> training_data = new List<IPosOnlyFeedback>();
		List<IPosOnlyFeedback> test_data = new List<IPosOnlyFeedback>();

		List<IPosOnlyFeedback> training_probe_data = new List<IPosOnlyFeedback>();
		List<IPosOnlyFeedback> test_probe_data = new List<IPosOnlyFeedback>();


		List<IList<int>> test_users = new List<IList<int>>();

		Dictionary<string, int> best_alg = new Dictionary<string, int>();

		// ID mapping objects
		protected List<IMapping> user_mapping = new List<IMapping>();
		protected List<IMapping> item_mapping = new List<IMapping>();

		// command line parameters
		protected string training_file;
		protected string test_file;
		protected string training_partial_file;
		protected string test_partial_file;


		protected OptionSet options;

		//Eval
		protected IList<string> eval_measures;
		protected string measures;


		//List of prediction probes
		List<IList<Tuple<int, float>>> list_prediction_probes = new List<IList<Tuple<int, float>>>();
		HashSet<int> correct_items_global = new HashSet<int>();

		Dictionary<string, List<double>> ga_weights = new Dictionary<string, List<double>>();



		protected string item_attributes_file;
		protected List<IBooleanMatrix> item_attributes = new List<IBooleanMatrix>();




		//
		string[] arquivos = new string[] { "genres", "tags", "directors", "actors", "countries" };
		//string[] arquivos = new string[] { "genres", "tags", "directors", "countries" };
		//string[] arquivos = new string[] { "genres", "countries"  };


		static public ICollection<string> Measures
		{
			get
			{
				string[] measures = { "AUC", "prec@5", "prec@10", "MAP", "recall@5", "recall@10", "NDCG", "MRR" };
				return new HashSet<string>(measures);
			}
		}

		static void Main(string[] args)
		{
			var program = new Program();
			program.Run(args);
		}

        
		protected void Run(string[] args)
		{
			Console.WriteLine("WISER-RecSys começou");

			options = new OptionSet() {
				// string-valued options
				{ "measures=",            v              => measures             = v },
			    { "recommender-options=", v              => recommender_options += " " + v },

			};

			eval_measures = ItemRecommendationEvaluationResults.DefaultMeasuresToShow;

			IList<string> extra_args = options.Parse(args);



			//eval
			if (measures != null)
				eval_measures = measures.Split(' ', ',');

			training_file = "training.data";
			test_file = "test.data";
			training_partial_file = "training.partial.data";
			test_partial_file = "test.partial.data";



			for (int i = 0; i < arquivos.Length; i++)
			{

				MyMediaLite.Random.Seed = 1;


				item_attributes_file = "movie_" + arquivos[i] + ".dat_saida";


				user_mapping.Add(new Mapping());
				item_mapping.Add(new Mapping());



				//Setup recommender
				recommenders.Add("BPRMFAttr".CreateItemRecommender());
				recommenders[i].Configure(recommender_options, (string msg) =>
				{
					Console.Error.WriteLine(msg); Environment.Exit(-1);
				});


				// item attributes
				if (recommenders[i] is IItemAttributeAwareRecommender && item_attributes_file == null)
					Abort("Recommender expects --item-attributes=FILE.");


				if (item_attributes_file != null)
					item_attributes.Add(AttributeData.Read(item_attributes_file, item_mapping[i]));
				if (recommenders[i] is IItemAttributeAwareRecommender)
					((IItemAttributeAwareRecommender)recommenders[i]).ItemAttributes = item_attributes[i];


				IBooleanMatrix lista_vazia = new SparseBooleanMatrix();
				if (recommenders[i] is IUserAttributeAwareRecommender)
					((IUserAttributeAwareRecommender)recommenders[i]).UserAttributes = lista_vazia;


				// training data
				training_data.Add(ItemData.Read(training_file, user_mapping[i], item_mapping[i], false));

				test_data.Add(ItemData.Read(test_file, user_mapping[i], item_mapping[i], false));


				test_users.Add(test_data[i].AllUsers);


				//Probe

				training_probe_data.Add(ItemData.Read(training_partial_file, user_mapping[i], item_mapping[i], false));
				test_probe_data.Add(ItemData.Read(test_partial_file, user_mapping[i], item_mapping[i], false));


				if (recommenders[i] is MyMediaLite.ItemRecommendation.ItemRecommender)
					((ItemRecommender)recommenders[i]).Feedback = training_probe_data[i];


				//Trainar
				Console.WriteLine("Vamos ao probe training");
				var train_time_span = Wrap.MeasureTime(delegate () { recommenders[i].Train(); });
				Console.WriteLine("training_time " + train_time_span + " ");


			}



			//Probe learn
			Console.WriteLine("Probe learn started");
			TimeSpan time_span = Wrap.MeasureTime(delegate () { EvaluateProbe(); });
			Console.WriteLine(" Probe learn time: " + time_span);


			for (int i = 0; i < arquivos.Length; i++)
			{

				MyMediaLite.Random.Seed = 1;


				item_attributes_file = "movie_" + arquivos[i] + ".dat_saida";


				//Setup recommender
				recommenders[i] = "BPRMFAttr".CreateItemRecommender();
				recommenders[i].Configure(recommender_options, (string msg) => { Console.Error.WriteLine(msg); Environment.Exit(-1); });


				// item attributes
				if (recommenders[i] is IItemAttributeAwareRecommender && item_attributes_file == null)
					Abort("Recommender expects --item-attributes=FILE.");


				if (recommenders[i] is IItemAttributeAwareRecommender)
					((IItemAttributeAwareRecommender)recommenders[i]).ItemAttributes = item_attributes[i];


				IBooleanMatrix lista_vazia = new SparseBooleanMatrix();
				if (recommenders[i] is IUserAttributeAwareRecommender)
					((IUserAttributeAwareRecommender)recommenders[i]).UserAttributes = lista_vazia;


				if (recommenders[i] is MyMediaLite.ItemRecommendation.ItemRecommender)
					((ItemRecommender)recommenders[i]).Feedback = training_data[i];



				//Trainar
				Console.WriteLine("Agora ao treino normal");
				var train_time_span = Wrap.MeasureTime(delegate () { recommenders[i].Train(); });
				Console.WriteLine("training_time " + train_time_span + " ");

			}



			var results = Evaluate();

			foreach (EvaluationResults result in results)
			{
				Console.WriteLine(result.ToString());
			}

			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();

		}


		protected void Abort(string message)
		{
			Console.Error.WriteLine(message);
			Environment.Exit(-1);
		}



		private List<KeyValuePair<int, float>> Ensenble(List<IList<Tuple<int, float>>> list)
		{
			var novaList = new Dictionary<int, float>();

			//Vamos criar uma nova lista com o score junto
			foreach (IList<Tuple<int, float>> aLista in list)
			{
				foreach (Tuple<int, float> item in aLista)
				{

					if (novaList.ContainsKey(item.Item1))
					{
						if (novaList[item.Item1] < item.Item2)
						{
							novaList[item.Item1] = item.Item2;
						}
					}
					else
					{
						novaList.Add(item.Item1, item.Item2);
					}

				}



			}

			List<KeyValuePair<int, float>> listRetorno = novaList.ToList();

			listRetorno.Sort((firstPair, nextPair) =>
			{
				return nextPair.Value.CompareTo(firstPair.Value);
			});


			return listRetorno;



		}

		//Usado no 
		private List<KeyValuePair<int, float>> EnsenblePeso(double[] pesos)
		{
			var novaList = new Dictionary<int, float>();

			//Vamos criar uma nova lista com o score junto
			for (int i = 0; i < list_prediction_probes.Count; i++)
			{
				foreach (Tuple<int, float> item in list_prediction_probes[i])
				{
					float value;
					if (novaList.TryGetValue(item.Item1, out value))
					{
						novaList[item.Item1] = value + (item.Item2 * (float)pesos[i]);

					}
					else
					{
						novaList.Add(item.Item1, item.Item2 * (float)pesos[i]);
					}

				}



			}

			List<KeyValuePair<int, float>> listRetorno = novaList.ToList();

			listRetorno.Sort((firstPair, nextPair) =>
			{
				return nextPair.Value.CompareTo(firstPair.Value);
			});


			return listRetorno;



		}


		public List<ItemRecommendationEvaluationResults> Evaluate(int n = -1)
		{
			List<IList<int>> candidate_items = new List<IList<int>>();
			List<RepeatedEvents> repeated_events = new List<RepeatedEvents>();
			List<IBooleanMatrix> training_user_matrix = new List<IBooleanMatrix>();
			List<IBooleanMatrix> test_user_matrix = new List<IBooleanMatrix>();


			for (int i = 0; i < recommenders.Count; i++)
			{

				candidate_items.Add(new List<int>(test_data[i].AllItems.Union(training_data[i].AllItems)));
				repeated_events.Add(RepeatedEvents.No);


				if (candidate_items[i] == null)
					throw new ArgumentNullException("candidate_items");
				if (test_users[i] == null)
					test_users[i] = test_data[i].AllUsers;

				training_user_matrix.Add(training_data[i].UserMatrix);
				test_user_matrix.Add(test_data[i].UserMatrix);
			}
			int num_users = 0;
			var result = new List<ItemRecommendationEvaluationResults>();

			for (int i = 0; i < recommenders.Count + 3; i++) // +Ensemble +GA
			{
				result.Add(new ItemRecommendationEvaluationResults());
			}

			// make sure that the user matrix is completely initialized before entering parallel code






			foreach (int user_id in test_users[0])
			{

				string original = user_mapping[0].ToOriginalID(user_id);


				List<IList<Tuple<int, float>>> list_of_predictions = new List<IList<Tuple<int, float>>>();

				HashSet<int> correct_items = new HashSet<int>();

				List<HashSet<int>> ignore_items_for_this_user = new List<HashSet<int>>();

				List<int> num_candidates_for_this_user = new List<int>();


				correct_items = new HashSet<int>(test_user_matrix[0][user_id]);
				correct_items.IntersectWith(candidate_items[0]);


				for (int i = 0; i < recommenders.Count; i++)
				{

					int internalId = user_mapping[i].ToInternalID(original);


					ignore_items_for_this_user.Add(new HashSet<int>(training_user_matrix[i][internalId]));



					/* if (correct_items[i].Count == 0)
                         continue;
                     */

					ignore_items_for_this_user[i].IntersectWith(candidate_items[i]);
					num_candidates_for_this_user.Add(candidate_items[i].Count - ignore_items_for_this_user[i].Count);
					/*if (correct_items[i].Count == num_candidates_for_this_user[i])
                        continue;
                    */


					//Recomenda


					var listaRecomendacao = recommenders[i].Recommend(user_id, candidate_items: candidate_items[i], n: n, ignore_items: ignore_items_for_this_user[i]);
					for (int j = 0; j < listaRecomendacao.Count; j++)
					{
						string idOriginal = item_mapping[i].ToOriginalID(listaRecomendacao[j].Item1);
						int idMappingZero = item_mapping[0].ToInternalID(idOriginal);


						Tuple<int, float> tupla = new Tuple<int, float>(idMappingZero, listaRecomendacao[j].Item2);

						listaRecomendacao[j] = tupla;
					}

					list_of_predictions.Add(listaRecomendacao);


				}




				//}

				//Nova
				//var prediction = Ensenble(list_of_predictions);
				//var prediction_list = (from t in prediction select t.Key).ToArray();






				for (int i = 0; i < recommenders.Count + 3; i++) // +Ensemble +GA
				{

					int best = best_alg[original];

					IList<int> prediction_list = null;
					int prediction_count = 0;


					if (i == list_of_predictions.Count)//Best of all
					{
						var prediction = list_of_predictions[best];
						prediction_list = (from t in prediction select t.Item1).ToArray();
						prediction_count = prediction.Count;
					}
					else if (i == list_of_predictions.Count + 1)//emsemble
					{
						var prediction_ensemble = Ensenble(list_of_predictions);

						prediction_list = (from t in prediction_ensemble select t.Key).ToArray();
						prediction_count = prediction_ensemble.Count;
					}
					else if (i == list_of_predictions.Count + 2)//GA
					{
						//Set global so Fitness itens can see.
						list_prediction_probes = list_of_predictions;
						correct_items_global = correct_items;

						var prediction_ensemble = EnsenblePeso(ga_weights[original].ToArray());

						prediction_list = (from t in prediction_ensemble select t.Key).ToArray();
						prediction_count = prediction_ensemble.Count;
					}
					else
					{
						var prediction = list_of_predictions[i];
						prediction_list = (from t in prediction select t.Item1).ToArray();
						prediction_count = prediction.Count;
					}




					int num_dropped_items = num_candidates_for_this_user[0] - prediction_count;
					double auc = AUC.Compute(prediction_list, correct_items, num_dropped_items);
					double map = PrecisionAndRecall.AP(prediction_list, correct_items);
					double ndcg = NDCG.Compute(prediction_list, correct_items);
					double rr = ReciprocalRank.Compute(prediction_list, correct_items);
					var positions = new int[] { 5, 10 };
					var prec = PrecisionAndRecall.PrecisionAt(prediction_list, correct_items, positions);
					var recall = PrecisionAndRecall.RecallAt(prediction_list, correct_items, positions);

					// thread-safe incrementing

					num_users++;
					result[i]["AUC"] += (float)auc;
					result[i]["MAP"] += (float)map;
					result[i]["NDCG"] += (float)ndcg;
					result[i]["MRR"] += (float)rr;
					result[i]["prec@5"] += (float)prec[5];
					result[i]["prec@10"] += (float)prec[10];
					result[i]["recall@5"] += (float)recall[5];
					result[i]["recall@10"] += (float)recall[10];


				}





				if (num_users % 1000 == 0)
					Console.Error.Write(".");
				if (num_users % 60000 == 0)
					Console.Error.WriteLine();

			}


			num_users /= recommenders.Count + 3;

			for (int i = 0; i < recommenders.Count + 3; i++) // +Ensemble +GA
			{
				foreach (string measure in Measures)
					result[i][measure] /= num_users;
				result[i]["num_users"] = num_users;
				result[i]["num_lists"] = num_users;
				result[i]["num_items"] = candidate_items.Count;
			}

			return result;
		}


		public static bool Terminate(Population population, int currentGeneration, long currentEvaluation)
		{
			return currentGeneration > 70;
		}

		private double CalculateFitness(Chromosome chromosome)
		{


			//get x and y from the solution

			double[] values = new double[list_prediction_probes.Count];
			double rangeConst = 1 / (System.Math.Pow(2, 10) - 1);

			for (int i = 0; i < list_prediction_probes.Count; i++)
			{
				string str = chromosome.ToBinaryString((i * 10), 10);
				Int64 convertInt32 = Convert.ToInt32(str, 2);

				double x = (convertInt32 * rangeConst);

				values[i] = x;
			}




			var result = EnsenblePeso(values);

			var prediction_ensemble_probe = (from t in result select t.Key).ToArray();

			double resultado_ensemble = PrecisionAndRecall.AP(prediction_ensemble_probe, correct_items_global);

			return resultado_ensemble;


		}



		public void EvaluateProbe(
		 int n = -1)
		{
			List<IList<int>> candidate_items = new List<IList<int>>();
			List<RepeatedEvents> repeated_events = new List<RepeatedEvents>();
			List<IBooleanMatrix> training_user_matrix = new List<IBooleanMatrix>();
			List<IBooleanMatrix> test_user_matrix = new List<IBooleanMatrix>();



			for (int i = 0; i < recommenders.Count; i++)
			{

				candidate_items.Add(new List<int>(test_probe_data[i].AllItems.Union(training_probe_data[i].AllItems)));
				repeated_events.Add(RepeatedEvents.No);


				if (candidate_items[i] == null)
					throw new ArgumentNullException("candidate_items");
				if (test_probe_data[i] == null)
					test_users[i] = test_probe_data[i].AllUsers;

				training_user_matrix.Add(training_probe_data[i].UserMatrix);
				test_user_matrix.Add(test_probe_data[i].UserMatrix);
			}
			int num_users = 0;
			var result = new ItemRecommendationEvaluationResults();

			// make sure that the user matrix is completely initialized before entering parallel code






			foreach (int user_id in test_users[0])
			{

				string original = user_mapping[0].ToOriginalID(user_id);


				List<IList<Tuple<int, float>>> list_of_predictions = new List<IList<Tuple<int, float>>>();

				HashSet<int> correct_items = new HashSet<int>();

				List<HashSet<int>> ignore_items_for_this_user = new List<HashSet<int>>();

				List<int> num_candidates_for_this_user = new List<int>();


				correct_items = new HashSet<int>(test_user_matrix[0][user_id]);
				correct_items.IntersectWith(candidate_items[0]);


				for (int i = 0; i < recommenders.Count; i++)
				{

					int internalId = user_mapping[i].ToInternalID(original);


					ignore_items_for_this_user.Add(new HashSet<int>(training_user_matrix[i][internalId]));



					/* if (correct_items[i].Count == 0)
                         continue;
                     */

					ignore_items_for_this_user[i].IntersectWith(candidate_items[i]);
					num_candidates_for_this_user.Add(candidate_items[i].Count - ignore_items_for_this_user[i].Count);
					/*if (correct_items[i].Count == num_candidates_for_this_user[i])
                        continue;
                    */


					//Recomenda


					var listaRecomendacao = recommenders[i].Recommend(user_id, candidate_items: candidate_items[i], n: n, ignore_items: ignore_items_for_this_user[i]);
					for (int j = 0; j < listaRecomendacao.Count; j++)
					{
						string idOriginal = item_mapping[i].ToOriginalID(listaRecomendacao[j].Item1);
						int idMappingZero = item_mapping[0].ToInternalID(idOriginal);


						Tuple<int, float> tupla = new Tuple<int, float>(idMappingZero, listaRecomendacao[j].Item2);

						listaRecomendacao[j] = tupla;
					}

					list_of_predictions.Add(listaRecomendacao);


				}



				//Usar o melhor
				double maiorMapping = 0;
				int idMaiorMapping = 0;

				//Testar cada individual
				for (int k = 0; k < list_of_predictions.Count; k++)
				{
					int[] prediction_probe = (from t in list_of_predictions[k] select t.Item1).ToArray();


					double resultado = PrecisionAndRecall.AP(prediction_probe, correct_items);

					if (resultado > maiorMapping)
					{
						maiorMapping = resultado;
						idMaiorMapping = k;

					}


				}

				//Set global so Fitness itens can see.
				list_prediction_probes = list_of_predictions;
				correct_items_global = correct_items;

				//Algortimo Genetico
				/*   //  Crossover		= 80%
                   //  Mutation		=  5%
                   //  Population size = 100
                   //  Generations		= 2000
                   //  Genome size		= 2
                   GA ga = new GA(0.8, 0.05, 40, 400, list_prediction_probes.Count);

                   ga.FitnessFunction = new GAFunction(Fitness);

                   //ga.FitnessFile = @"H:\fitness.csv";
                   ga.Elitism = true;
                   ga.Go();

                   double[] values;
                   double fitness;
                   ga.GetBest(out values, out fitness);*/

				//create the GA using an initialised population and user defined Fitness Function 
				const double crossoverProbability = 0.85;
				const double mutationProbability = 0.08;
				const int elitismPercentage = 5;

				//create a Population of random chromosomes of length 44 
				var population = new Population(40, list_of_predictions.Count * 10, false, false);

				//create the genetic operators 
				var elite = new Elite(elitismPercentage);
				var crossover = new Crossover(crossoverProbability, true)
				{
					CrossoverType = CrossoverType.DoublePoint
				};
				var mutation = new BinaryMutate(mutationProbability, true);

				//create the GA itself 
				var ga = new GeneticAlgorithm(population, CalculateFitness);

				//add the operators to the ga process pipeline 
				ga.Operators.Add(elite);
				ga.Operators.Add(crossover);
				ga.Operators.Add(mutation);

				//run the GA 
				ga.Run(Terminate);


				var best = population.GetTop(1)[0];
				double rangeConst = 1 / (System.Math.Pow(2, 10) - 1);
				ga_weights[original] = new List<double>();

				for (int i = 0; i < list_prediction_probes.Count; i++)
				{
					string str = best.ToBinaryString((i * 10), 10);
					Int64 convertInt32 = Convert.ToInt32(str, 2);

					double x = (convertInt32 * rangeConst);

					ga_weights[original].Add(x);
				}







				//Testar Ensemble
				/*
                var prediction_ensemble = Ensenble(list_of_predictions);

                var prediction_ensemble_probe = (from t in prediction_ensemble select t.Key).ToArray();

                double resultado_ensemble = PrecisionAndRecall.AP(prediction_ensemble_probe, correct_items);

                if (resultado_ensemble > maiorMapping)
                {
                    maiorMapping = resultado_ensemble;
                    idMaiorMapping = list_of_predictions.Count; // ultimo id

                }*/












				best_alg[original] = idMaiorMapping;
				num_users++;


				if (num_users % 10 == 0)
					Console.Error.Write(".");
				if (num_users % 100 == 0)
					Console.Error.WriteLine("");


			}


		}

	}
}
