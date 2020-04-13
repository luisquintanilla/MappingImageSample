using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Text.Json.Serialization;
using Microsoft.ML;
using MappingImageSampleML.Model;

namespace MappingImageSampleAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ClassificationController : ControllerBase
    {
        private readonly PredictionEngine<ModelInput, ModelOutput> _predictionEngine;
        private readonly object _predictionEngineLock = new object();

        public ClassificationController(PredictionEngine<ModelInput,ModelOutput> predictionEngine)
        {
            _predictionEngine = predictionEngine;
        }

        [HttpPost]
        public async Task<string> ClassifyImage([FromBody] Dictionary<string,string> input)
        {
            string prediction;
            string imagePath = "output.jpeg";
            
            // Convert Base64 string to Image Bytes
            var imageBytes = Convert.FromBase64String(input["data"]);
                
            // Write bytes to image
            using(var ms = new MemoryStream(imageBytes))
            {
                // Save file
                using (var img = await Task.Run(() => Image.FromStream(ms)))
                    await Task.Run(() => img.Save(imagePath));
                
                // Get Prediction
                lock (_predictionEngineLock)
                {
                    ModelOutput output = _predictionEngine.Predict(new ModelInput { ImageSource = imagePath });
                    prediction = output.Prediction;   
                }
            }

            return prediction;
        }
    }
}