using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exercise1.Model;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;

namespace Exercise1.Controller
{
    public class CategoryModel
    {
        #region Members
        public string CategoryName;

        public Matrix<double> QMatrix;

        public Matrix<double> PMatrix;

        public Vector<double> Bu;

        public Vector<double> Bi;

        public double Regularization;

        public double LearningRate;

        public int LatentFeatures;

        public double Miu;
        
        public Dictionary<User, List<Rating>> TrainDataByUser;
        
        public Dictionary<User, List<Rating>> ValidationDataByUser;
        
        public Dictionary<int, User> AllUsersByIndexes;
        
        public Dictionary<int, Item> AllItemsByIndexes;

        public double MinTrainRMSE;

        public double MinTestRMSE;

        public double MinTestMAE;
        #endregion

        public CategoryModel(DataManager dataManager, string categoryName, int latentFeatures, double lRate, double regularization,
            Dictionary<User, List<Rating>> trainDataByUser, Dictionary<User, List<Rating>> validationDataByUser, Dictionary<int, User> allUsersByIndexes,
            Dictionary<int, Item> allItemsByIndexes)
        {
            TrainDataByUser = trainDataByUser;
            ValidationDataByUser = validationDataByUser;
            AllUsersByIndexes = allUsersByIndexes;
            AllItemsByIndexes = allItemsByIndexes;
            LatentFeatures = latentFeatures;
            LearningRate = lRate;
            Regularization = regularization;
            CategoryName = categoryName;

            PMatrix = Matrix<double>.Build.Random(AllUsersByIndexes.Count, LatentFeatures, new ContinuousUniform(0.1, 0.2));
            QMatrix = Matrix<double>.Build.Random(LatentFeatures, AllItemsByIndexes.Count, new ContinuousUniform(0.1, 0.2));

            Bu = Vector<double>.Build.Random(AllUsersByIndexes.Count, new ContinuousUniform(0.01, 0.01));
            Bi = Vector<double>.Build.Random(AllItemsByIndexes.Count, new ContinuousUniform(0.01, 0.01));

            Miu = TrainDataByUser.SelectMany(x => x.Value).Average(a => a.RatingNumber);

        }
    }
}
