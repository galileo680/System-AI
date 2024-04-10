using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Cryptography;
using SystemAI.Interfaces;
using SystemAI.Services;

namespace SystemAI.Controllers
{
    [Route("api")]
    [ApiController]
    public class AlgorithmsController : ControllerBase
    {

        private readonly IAlgorithmService _algorithmService;
        private readonly IFitnessFunctionService _fitnessFunction;
        private CalculationStateService _calculationStateService = new CalculationStateService();

        public AlgorithmsController(IAlgorithmService algorithmService, IFitnessFunctionService fitnessFunctionService)
        {
            _algorithmService = algorithmService;
            _fitnessFunction = fitnessFunctionService;
        }

        // GET: api/algorithms
        [HttpGet("Algorithms")]
        public ActionResult<IEnumerable<string>> GetAllAlgorithms()
        {
            var algorithms = _algorithmService.GetAllAlgorithmNames();
            return Ok(algorithms);
        }


        // POST: api/algorithms/run
        [HttpPost("run")]
        public ActionResult RunAlgorithm([FromBody] AlgorithmRunRequest request)
        {
            try
            {
                _calculationStateService.isFinished = false;
                var result = _algorithmService.RunAlgorithm(request.AlgorithmName, request.FitnessFunctions, request.Parameters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/algorithms/run-multiple
        [HttpPost("run-multiple")]
        public ActionResult RunAlgorithms([FromBody] MultipleAlgorithmsRunRequest request)
        {
            try
            {
                var result = _algorithmService.RunAlgorithms(request.Algorithms, request.FitnessFunction, request.Population, request.Iteration);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Nowe metody
        [HttpPost("addAlgorithm")]
        public async Task<IActionResult> AddAlgorithm([FromQuery] string name, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file with algorithm provided.");
            }

            try
            {
                byte[] algorithmData;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    algorithmData = memoryStream.ToArray();
                }

                _algorithmService.AddAlgorithm(name, algorithmData);
                return Ok("Algorithm added successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("updateAlgorithm/{name}")]
        public IActionResult UpdateAlgorithm(string name, [FromBody] byte[] algorithmData)
        {
            try
            {
                _algorithmService.UpdateAlgorithm(name, algorithmData);
                return Ok("Algorithm updated successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("deleteAlgorithm/{name}")]
        public IActionResult DeleteAlgorithm(string name)
        {
            try
            {
                _algorithmService.DeleteAlgorithm(name);
                return Ok("Algorithm deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ParamsInfo")]
        public IActionResult GetParamsInfo(string algorithmName)
        {
            try
            {
                var response = _algorithmService.GetParamsInfo(algorithmName);
                //var result = _algorithmService.GetParamsInfo(algorithmName);
                //return Ok(result);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("stopCalculations")]
        public IActionResult StopCalculations()
        {
            _calculationStateService.stopCalcFlag = true;
            return Ok("Calculations stopped.");
        }

        [HttpPost("resumeCalculations")]
        public IActionResult ResumeCalculations()
        {
            //_algorithmService.ResumeCalculation();
            return Ok("Calculations resumed.");
        }

        //GTOA //TSFDE
        [HttpPost("TSFDE")]
        public IActionResult TSFDE([FromBody] tsfdeRequest request)
        {
            try
            {
                var result = _algorithmService.RunAlgorithmTSFDE(request.AlgorithmName, request.Domain, request.Parameters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Fitness function

        // GET: api/Functions
        [HttpGet("Functions")]
        public ActionResult<IEnumerable<string>> GetAllFunctions()
        {
            var functions = _fitnessFunction.GetAllFitnessFunctionNames();
            return Ok(functions);
        }

        [HttpPost("addFitnessFunction")]
        public async Task<IActionResult> AddFitnessFunction([FromQuery] string name, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file with fitness function provided.");
            }

            try
            {
                byte[] fitnessFunctionData;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fitnessFunctionData = memoryStream.ToArray();
                }

                _fitnessFunction.AddFitnessFunction(name, fitnessFunctionData);
                return Ok("Fitness funtion added successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }


    public class FitnessFunctionRequest
    {
        public string Name { get; set; }
        public string Domain { get; set; } // Dane jako string JSON
    }

    public class AlgorithmRunRequest
    {
        public string AlgorithmName { get; set; }
        public List<FitnessFunctionRequest> FitnessFunctions { get; set; }
        public double[] Parameters { get; set; }
    }

    public class tsfdeRequest
    {
        public string AlgorithmName { get; set; }
        //public double[] a { get; set; }
        //public double[] b { get; set; }
        public string Domain { get; set; }
        //public int population { get; set; }
        //public int iterations { get; set; }
        public double[] Parameters { get; set; }
    }

    public class AlgorithmRequest
    {
        public string Name { get; set; }
        //public double[] Parameters { get; set; } //c1 c2 ...
        public double[] Steps { get; set; }
    }

    public class MultipleAlgorithmsRunRequest
    {
        public List<AlgorithmRequest> Algorithms { get; set; }
        public FitnessFunctionRequest FitnessFunction { get; set; }
        public double Population { get; set; }
        public double Iteration { get; set; }

    }

    public class CalculationStateService
    {
        public bool stopCalcFlag { get; set; } = false;
        public bool isFinished { get; set; } = true;
    }
}
