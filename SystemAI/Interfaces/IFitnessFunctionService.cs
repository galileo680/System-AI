namespace SystemAI.Interfaces
{
    public interface IFitnessFunctionService
    {
        IEnumerable<string> GetAllFitnessFunctionNames();
        void AddFitnessFunction(string functionName, byte[] functionData);
        void UpdateFitnessFunction(string functionName, byte[] functionData);
        void DeleteFitnessFunction(string functionName);
    }
}
