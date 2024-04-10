using SystemAI.Interfaces;

namespace SystemAI.Services
{
    public class FitnessFunctionService : IFitnessFunctionService
    {
        private readonly string _fitnessFunctionFolderPath;
        public FitnessFunctionService(string fitnessFunctionFolderPath)
        {
            _fitnessFunctionFolderPath = fitnessFunctionFolderPath;
        }

        public IEnumerable<string> GetAllFitnessFunctionNames()
        {
            return Directory.GetFiles(_fitnessFunctionFolderPath, "*.dll").Select(Path.GetFileNameWithoutExtension);
        }

        public void AddFitnessFunction(string functionName, byte[] functionData)
        {
            string filePath = Path.Combine(_fitnessFunctionFolderPath, functionName + ".dll");
            File.WriteAllBytes(filePath, functionData);
        }

        public void UpdateFitnessFunction(string functionName, byte[] functionData)
        {
            throw new NotImplementedException();
        }

        public void DeleteFitnessFunction(string functionName)
        {
            throw new NotImplementedException();
        }
    }
}
