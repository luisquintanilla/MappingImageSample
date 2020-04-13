// This file was auto-generated by ML.NET Model Builder. 

using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using System;
using System.Linq;

namespace MappingImageSampleML.Model
{
    [CustomMappingFactoryAttribute(nameof(NormalizeMapping))]
    public class NormalizeMapping : CustomMappingFactory<NormalizeInput, NormalizeOutput>
    {
        // This is the custom mapping. We now separate it into a method, so that we can use it both in training and in loading.
        public static void Mapping(NormalizeInput input, NormalizeOutput output)
        {
            var values = input.Reshape.GetValues().ToArray();

            var image_mean = new float[] { 0.485f, 0.456f, 0.406f };
            var image_std = new float[] { 0.229f, 0.224f, 0.225f };

            for (int x = 0; x < values.Count(); x++)
            {
                var y = x % 3;
                // Normalize by 255 first
                values[x] /= 255;
                values[x] = (values[x] - image_mean[y]) / image_std[y];
            };

            output.Reshape = new VBuffer<float>(values.Count(), values);
        }
        // This factory method will be called when loading the model to get the mapping operation.
        public override Action<NormalizeInput, NormalizeOutput> GetMapping()
        {
            return Mapping;
        }
    }
    public class NormalizeInput
    {
        [ColumnName("ImageSource_featurized")]
        [VectorType(3, 224, 224)]
        public VBuffer<float> Reshape;
    }
    public class NormalizeOutput
    {
        [ColumnName("input1")]
        [VectorType(3 * 224 * 224)]
        public VBuffer<float> Reshape;
    }
}
