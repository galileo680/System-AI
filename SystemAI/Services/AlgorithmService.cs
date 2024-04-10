using Newtonsoft.Json;
using System.Reflection;
using System.Xml.Linq;
using SystemAI.Controllers;
using SystemAI.Interfaces;

namespace SystemAI.Services
{
    public class AlgorithmService : IAlgorithmService
    {

        private readonly string _algorithmsFolderPath;
        private readonly AlgorithmLoader _algorithmLoader;
        private readonly string _fitnessFunctionPath;
        private readonly FitnesFunctionLoader _fitnesFunctionLoader;

        public AlgorithmService(string algorithmsFolderPath, AlgorithmLoader algorithmLoader, string fitnessFunctionPath, FitnesFunctionLoader fitnesFunctionLoader)
        {
            _algorithmsFolderPath = algorithmsFolderPath;
            _algorithmLoader = algorithmLoader;
            _fitnessFunctionPath = fitnessFunctionPath;
            _fitnesFunctionLoader = fitnesFunctionLoader;
        }

        public IEnumerable<string> GetAllAlgorithmNames()
        {
            return Directory.GetFiles(_algorithmsFolderPath, "*.dll").Select(Path.GetFileNameWithoutExtension);
        }

        public (object, Type) LoadAlgorithm(string algorithmName)
        {
            string dllPath = Path.Combine(_algorithmsFolderPath, algorithmName + ".dll");
            return _algorithmLoader.LoadAlgorithm(dllPath);
        }

        public List<object> LoadFitnessFunctions(string[] functionNames)
        {
            string[] dllPaths = new string[functionNames.Length];
            for (int i = 0; i < functionNames.Length; i++)
            {
                string dllPath = Path.Combine(_fitnessFunctionPath, functionNames[i] + ".dll");
                dllPaths[i] = dllPath;
            }

            return _fitnesFunctionLoader.LoadFitnesFunctions(dllPaths);
        }

        public object RunAlgorithm(string algorithmName, List<FitnessFunctionRequest> fitnessFunctionsRequest, params double[] parameters)
        {
            (var optimizationAlgorithm, var delegateFunction) = LoadAlgorithm(algorithmName);
            if (optimizationAlgorithm == null)
            {
                throw new InvalidOperationException("Algorithm could not be loaded.");
            }
            else if (delegateFunction == null)
            {
                throw new InvalidOperationException("Delegate Function could not be loaded.");
            }

            var fitnessFunctionNames = fitnessFunctionsRequest.Select(request => request.Name).ToArray();
            var fitnessFunctions = LoadFitnessFunctions(fitnessFunctionNames);

            if (fitnessFunctions == null)
            {
                throw new InvalidOperationException("Fitness functions could not be loaded.");
            }

            var domains = new List<double[,]>();

            foreach (var request in fitnessFunctionsRequest)
            {
                var deserializedDomain = DeserializeDomain(request.Domain);
                domains.Add(deserializedDomain);
            }
            // double[,] domain = DeserializeDomain(DomainSerialized);


            // Wypisywanie wartości domain
            /*Console.WriteLine("Domain Values:");
            for (int i = 0; i < domain.GetLength(0); i++)
            {
                for (int j = 0; j < domain.GetLength(1); j++)
                {
                    Console.Write(domain[i, j] + " ");
                }
                Console.WriteLine();
            }*/

            // Wypisywanie wartości parameters
            Console.WriteLine("Parameters:");
            foreach (var param in parameters)
            {
                Console.Write(param + " ");
            }
            Console.WriteLine();

            var solve = optimizationAlgorithm.GetType().GetMethod("Solve");

            List<object> response = new List<object> { };

            for (int i = 0; i < fitnessFunctions.Count(); i++)
            {
                var calculateMethodInfo = fitnessFunctions[i].GetType().GetMethod("Function");
                var calculate = Delegate.CreateDelegate(delegateFunction, fitnessFunctions[i], calculateMethodInfo);
                solve.Invoke(optimizationAlgorithm, new object[] { calculate, domains[i], parameters });

                var XBest = optimizationAlgorithm.GetType().GetProperty("XBest");
                var FBest = optimizationAlgorithm.GetType().GetProperty("FBest");
                var NumberOfEvaluationFitnessFunction = optimizationAlgorithm.GetType().GetProperty("NumberOfEvaluationFitnessFunction");

                var XBestValue = (double[])XBest.GetValue(optimizationAlgorithm);
                var FBestValue = (double)FBest.GetValue(optimizationAlgorithm);
                var NumberOfEvaluationFitnessFunctionValue = (int)NumberOfEvaluationFitnessFunction.GetValue(optimizationAlgorithm);

                Console.WriteLine(XBestValue);
                Console.WriteLine(FBestValue);
                Console.WriteLine(NumberOfEvaluationFitnessFunctionValue);

                var algorithmResult = new
                {
                    XBestValue = XBestValue,
                    FBestValue = FBestValue,
                    NumberOfEvaluationFitnessFunctionValue = NumberOfEvaluationFitnessFunctionValue
                };

                response.Add(algorithmResult);

                //Generowanie pdfa
                var pdfReportGenerator = optimizationAlgorithm.GetType().GetProperty("pdfReportGenerator").GetValue(optimizationAlgorithm);
                
                var GenerateReport = pdfReportGenerator.GetType().GetMethod("GenerateReport");

                //var path = Path.Combine(Directory.GetCurrentDirectory(), "Reports");

                var fitnessFunctionName = fitnessFunctionNames.GetValue(i);
                var path = fitnessFunctionName + "-" + algorithmName + "-" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "-" + "Raport.pdf";

                try
                {
                    GenerateReport.Invoke(pdfReportGenerator, new object[] { path });
                }
                catch (TargetInvocationException ex)
                {
                    Console.WriteLine("Szczegóły wyjątku: " + ex.InnerException.Message);
                }

            //usuwanie json
            /* 
             string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Data");

             // Pobiera wszystkie pliki .json w folderze
             string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");

             // Iteruje przez wszystkie znalezione pliki i je usuwa
             foreach (string file in jsonFiles)
             {
                 File.Delete(file);
             }*/

            }

            object resposneObject = new { response };

            return resposneObject;
        }

        public object RunAlgorithms(List<AlgorithmRequest> algorithms, FitnessFunctionRequest fitnessFunctionRequest, double population, double iteration)
        {

            var algorithmNames = new List<string>();

            foreach(var algorithm in algorithms)
            {
                algorithmNames.Add(algorithm.Name);
            }

            List<(object, Type)> optimizationAlgorithms = new List<(object, Type)>();

            foreach (var name in algorithmNames)
            {
                (var optimizationAlgorithm, var delegateFunction) = LoadAlgorithm(name);
                if (optimizationAlgorithm != null && delegateFunction != null)
                {
                    optimizationAlgorithms.Add((optimizationAlgorithm, delegateFunction));
                }
                else
                {
                    throw new InvalidOperationException("Algorithm or Delegate Function could not be loaded.");
                }
            }

            var fitnessFunction = LoadFitnessFunctions(new string[] { fitnessFunctionRequest.Name });

            if (fitnessFunction == null)
            {
                throw new InvalidOperationException("Fitness function could not be loaded.");
            }
          
            double[,] domain = DeserializeDomain(fitnessFunctionRequest.Domain);


            var algorithmsCombinations = new List<Algorithm>();

            var i = 0;
            foreach (var optimizationAlgorithm in optimizationAlgorithms)
            {
                //optimizationAlgorithm.Item1
                var _name = (string)optimizationAlgorithm.Item1.GetType().GetProperty("Name").GetValue(optimizationAlgorithm.Item1);
                var paramsInfoArray = (Array)optimizationAlgorithm.Item1.GetType().GetProperty("ParamsInfo").GetValue(optimizationAlgorithm.Item1);
                //algorithms[0].Steps[0];
                List<AlgorithmParameter> paramInfoRequests = new List<AlgorithmParameter>();

                var y = 0;
                foreach (var paramInfo in paramsInfoArray)
                {                   
                    double _upperBoundary = (double)paramInfo.GetType().GetProperty("UpperBoundary").GetValue(paramInfo);
                    double _lowerBoundary = (double)paramInfo.GetType().GetProperty("LowerBoundary").GetValue(paramInfo);                    


                    paramInfoRequests.Add(new AlgorithmParameter { UpperBoundary = _upperBoundary, LowerBoundary = _lowerBoundary, Step = algorithms[i].Steps[y] });
                    y++;
                }

                i++;
                algorithmsCombinations.Add(new Algorithm { Name = _name, Parameters = paramInfoRequests });
            }


            Dictionary<string, List<double[]>> allCombinations = new Dictionary<string, List<double[]>>();
            try
            {
                allCombinations = GenerateAllCombinations(algorithmsCombinations);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //Wypisanie do konsoli wszsytkich mozliwych kombinacji
            foreach (var algorithm in allCombinations)
            {
                Console.WriteLine($"Algorytm: {algorithm.Key}");

                foreach (var combination in algorithm.Value)
                {
                    string formattedCombination = string.Join(", ", combination.Select(c => c.ToString("F2"))); // F2 oznacza, że liczby będą formatowane z dwoma miejscami po przecinku
                    Console.WriteLine($"Kombinacja: [{formattedCombination}]");
                }

                Console.WriteLine(); 
            }

            List<AlgorithmResult> bestResults = new List<AlgorithmResult>();

            foreach (var (algorithm, delegateFunction) in optimizationAlgorithms)
            {
                string currentName = (string)algorithm.GetType().GetProperty("Name").GetValue(algorithm);

                var solve = algorithm.GetType().GetMethod("Solve");

                var calculateMethodInfo = fitnessFunction[0].GetType().GetMethod("Function");
                var calculate = Delegate.CreateDelegate(delegateFunction, fitnessFunction[0], calculateMethodInfo);

                double bestF = double.MaxValue; // Zakładając, że im mniejsza wartość F, tym lepsza
                double[] bestX = null;
                double[] bestParameters = null;

                foreach (var combination in allCombinations[currentName])
                {
                    // Rozszerzenie tablicy o wartości iteracji i populacji
                    double[] extendedCombination = new double[combination.Length + 2];
                    combination.CopyTo(extendedCombination, 0);
                    extendedCombination[combination.Length] = population;
                    extendedCombination[combination.Length + 1] = iteration;
                    solve.Invoke(algorithm, new object[] { calculate, domain, extendedCombination });

                    var XBest = algorithm.GetType().GetProperty("XBest");
                    var FBest = algorithm.GetType().GetProperty("FBest");
                    

                    var XBestValue = (double[])XBest.GetValue(algorithm);
                    var FBestValue = (double)FBest.GetValue(algorithm);
                    

                    if (FBestValue < bestF)
                    {
                        bestF = FBestValue;
                        bestX = XBestValue;
                        bestParameters = combination;
                    }
                }

                bestResults.Add(new AlgorithmResult(currentName, bestParameters, bestF, bestX));

                //Generowanie pdfa
                var pdfReportGenerator = algorithm.GetType().GetProperty("pdfReportGenerator").GetValue(algorithm);

                var GenerateReport = pdfReportGenerator.GetType().GetMethod("GenerateReport");

                //var path = Path.Combine(Directory.GetCurrentDirectory(), "Reports");

                var fitnessFunctionName = fitnessFunctionRequest.Name;
                var path = fitnessFunctionName + "-" + currentName + "-" + "ResultForBestParams" + "-" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "-" + "Raport.pdf";

                try
                {
                    GenerateReport.Invoke(pdfReportGenerator, new object[] { path });
                }
                catch (TargetInvocationException ex)
                {
                    Console.WriteLine("Szczegóły wyjątku: " + ex.InnerException.Message);
                }
            }

            return (object)bestResults;

        }

        public object RunAlgorithmTSFDE(string algorithmName, string Domain, params double[] Parameteres)
        {
            (var optimizationAlgorithm, var delegateFunction) = LoadAlgorithm(algorithmName);
            if (optimizationAlgorithm == null)
            {
                throw new InvalidOperationException("Algorithm could not be loaded.");
            }
            else if (delegateFunction == null)
            {
                throw new InvalidOperationException("Delegate Function could not be loaded.");
            }

            //object fitnessFunction;

            var _Domain = DeserializeDomain(Domain);

            var fitnessFunctionFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Functions");
            string dllPath = Path.Combine(fitnessFunctionFolderPath, "TSFDE.dll");

            Assembly assembly = Assembly.LoadFrom(dllPath);

            Type t = assembly.GetType("TSFDE_fractional_boundary_condition.TSFDE_fractional_boundary");
            var fitnessFunction = Activator.CreateInstance(t);

            var solve = optimizationAlgorithm.GetType().GetMethod("Solve");

            var calculateMethodInfo = fitnessFunction.GetType().GetMethod("fintnessFunction");
            var calculate = Delegate.CreateDelegate(delegateFunction, fitnessFunction, calculateMethodInfo);
            solve.Invoke(optimizationAlgorithm, new object[] { calculate, _Domain, Parameteres });

            var XBest = optimizationAlgorithm.GetType().GetProperty("XBest");
            var FBest = optimizationAlgorithm.GetType().GetProperty("FBest");
            var NumberOfEvaluationFitnessFunction = optimizationAlgorithm.GetType().GetProperty("NumberOfEvaluationFitnessFunction");

            var XBestValue = (double[])XBest.GetValue(optimizationAlgorithm);
            var FBestValue = (double)FBest.GetValue(optimizationAlgorithm);
            var NumberOfEvaluationFitnessFunctionValue = (int)NumberOfEvaluationFitnessFunction.GetValue(optimizationAlgorithm);

            Console.WriteLine(XBestValue);
            Console.WriteLine(FBestValue);
            Console.WriteLine(NumberOfEvaluationFitnessFunctionValue);


            //Generowanie pdfa
            var pdfReportGenerator = optimizationAlgorithm.GetType().GetProperty("pdfReportGenerator").GetValue(optimizationAlgorithm);

            var GenerateReport = pdfReportGenerator.GetType().GetMethod("GenerateReport");
            var path = "TSFDE" + "-" + algorithmName + "-" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "-" + "Raport.pdf";

            try
            {
                GenerateReport.Invoke(pdfReportGenerator, new object[] { path });
            }
            catch (TargetInvocationException ex)
            {
                Console.WriteLine("Szczegóły wyjątku: " + ex.InnerException.Message);
            }

            return null;
        }

        // Nowe metody
        public void AddAlgorithm(string algorithmName, byte[] algorithmData)
        {
            string filePath = Path.Combine(_algorithmsFolderPath, algorithmName + ".dll");
            File.WriteAllBytes(filePath, algorithmData);
        }

        public void UpdateAlgorithm(string algorithmName, byte[] algorithmData)
        {
            string filePath = Path.Combine(_algorithmsFolderPath, algorithmName + ".dll");
            if (File.Exists(filePath))
            {
                File.WriteAllBytes(filePath, algorithmData);
            }
            else
            {
                throw new FileNotFoundException("Algorithm DLL not found.");
            }
        }

        public void DeleteAlgorithm(string algorithmName)
        {
            string filePath = Path.Combine(_algorithmsFolderPath, algorithmName + ".dll");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            else
            {
                throw new FileNotFoundException("Algorithm DLL not found.");
            }
        }

        public List<ParamInfoResponse> GetParamsInfo(string algorithmName)
        {
            (var optimizationAlgorithm, var delegateFunction) = LoadAlgorithm(algorithmName);
            if (optimizationAlgorithm == null)
            {
                throw new InvalidOperationException("Algorithm could not be loaded.");
            }
            else if (delegateFunction == null)
            {
                throw new InvalidOperationException("Delegate Function could not be loaded.");
            }

            // Pobieranie właściwości ParamsInfo          

            var paramsInfoArray = (Array)optimizationAlgorithm.GetType().GetProperty("ParamsInfo").GetValue(optimizationAlgorithm);

            List<ParamInfoResponse> paramInfoRequests = new List<ParamInfoResponse>();
            if(paramsInfoArray != null)
            {

            foreach (var paramInfo in paramsInfoArray)
            {
                string _name = (string)paramInfo.GetType().GetProperty("Name").GetValue(paramInfo);
                string _description = (string)paramInfo.GetType().GetProperty("Description").GetValue(paramInfo);
                double _upperBoundary = (double)paramInfo.GetType().GetProperty("UpperBoundary").GetValue(paramInfo);
                double _lowerBoundary = (double)paramInfo.GetType().GetProperty("LowerBoundary").GetValue(paramInfo);
                //double _step = (double)paramInfo.GetType().GetProperty("Step").GetValue(paramInfo);


                paramInfoRequests.Add(new ParamInfoResponse(_name, _description, _lowerBoundary, _upperBoundary));               
            }
            }

            Console.WriteLine(paramInfoRequests);
            return paramInfoRequests;

        }

        public List<double[]> GenerateCombinations(Algorithm algorithm)
        {
            return GenerateCombinationsRecursive(algorithm.Parameters, new List<double>(), 0);
        }

        private List<double[]> GenerateCombinationsRecursive(List<AlgorithmParameter> parameters, List<double> currentCombination, int currentIndex)
        {
            List<double[]> combinations = new List<double[]>();

            if (currentIndex == parameters.Count)
            {
                combinations.Add(currentCombination.ToArray());
                return combinations;
            }

            AlgorithmParameter currentParam = parameters[currentIndex];
            for (double value = currentParam.LowerBoundary; value <= currentParam.UpperBoundary; value += currentParam.Step)
            {
                List<double> newCombination = new List<double>(currentCombination) { value };
                combinations.AddRange(GenerateCombinationsRecursive(parameters, newCombination, currentIndex + 1));
            }

            return combinations;
        }

        public Dictionary<string, List<double[]>> GenerateAllCombinations(List<Algorithm> Algorithms)
        {
            Dictionary<string, List<double[]>> allCombinations = new Dictionary<string, List<double[]>>();

            foreach (Algorithm algorithm in Algorithms)
            {
                List<double[]> combinations = GenerateCombinations(algorithm);
                allCombinations.Add(algorithm.Name, combinations);
            }

            return allCombinations;
        }

        private double[,] DeserializeDomain(string serializedDomain)
        {
            // Deserializacja do listy list lub innego odpowiedniego formatu
            var domainList = JsonConvert.DeserializeObject<List<List<double>>>(serializedDomain);

            // Konwersja listy list na tablicę dwuwymiarową
            int rows = domainList.Count;
            int cols = domainList[0].Count;
            double[,] domainArray = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    domainArray[i, j] = domainList[i][j];
                }
            }

            return domainArray;
        }

        
    }

    public class AlgorithmLoader
    {
        public (object, Type) LoadAlgorithm(string dllPath)
        {
            object instance = null;
            Type delegateFunction = null;
            Assembly assembly = Assembly.LoadFrom(dllPath);

            foreach (Type t in assembly.GetTypes())
            {
                //Console.WriteLine(t.FullName);
                // Szukanie klas ktore implementuja IOptimizationAlgorithm
                if (t.GetInterface("IOptimizationAlgorithm", true) != null)
                {
                    Console.WriteLine("Found Type: {0}", t.FullName);

                    instance = Activator.CreateInstance(t);

                    break;
                }
            }

            delegateFunction = assembly.GetType("fitnessFunction");           

            return (instance, delegateFunction);
            //throw new InvalidOperationException("No valid algorithm found in DLL.");
        }
    }

    public class FitnesFunctionLoader
    {       
        public List<object> LoadFitnesFunctions(string[] fitnessFunctionsNames)
        {
            List<object> fitnessFunctions = new List<object>();

            foreach (var fitnessFunction in fitnessFunctionsNames)
            {
                Assembly assembly = Assembly.LoadFrom(fitnessFunction);

                foreach (Type t in assembly.GetTypes())
                {
                    if (t.GetInterface("FitnesFunction", true) != null)
                    {
                        Console.WriteLine("Found Fitnes Function: {0}", t.FullName);

                        var instance = Activator.CreateInstance(t);

                        fitnessFunctions.Add(instance);
                    }
                }
            }

            return fitnessFunctions;
        }
    }

    public class ParamInfoResponse
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double UpperBoundary { get; set; }
        public double LowerBoundary { get; set; }
        

        public ParamInfoResponse(string _name, string _description, double _lowerBoundary, double _uppderBoundary)
        {
            Name = _name;
            Description = _description;
            UpperBoundary = _uppderBoundary;
            LowerBoundary = _lowerBoundary;
        }

    }

    public class AlgorithmParameter
    {
        public double LowerBoundary { get; set; }
        public double UpperBoundary { get; set; }
        public double Step { get; set; }
    }

    public class Algorithm
    {
        public string Name { get; set; }
        public List<AlgorithmParameter> Parameters { get; set; }
    }

    public class AlgorithmResult
    {
        public string AlgorithmName { get; set; }
        public double[] BestParameters { get; set; }
        public double BestF { get; set; }
        public double[] BestX { get; set; }

        public AlgorithmResult(string algorithmName, double[] bestParameters, double bestF, double[] bestX)
        {
            AlgorithmName = algorithmName;
            BestParameters = bestParameters;
            BestF = bestF;
            BestX = bestX;
        }
    }
}

