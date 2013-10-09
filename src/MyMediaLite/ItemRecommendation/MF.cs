// Copyright (C) 2011, 2012, 2013 Zeno Gantner
// Copyright (C) 2010 Steffen Rendle, Zeno Gantner, Christoph Freudenthaler
//
// This file is part of MyMediaLite.
//
// MyMediaLite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MyMediaLite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MyMediaLite.DataType;
using MyMediaLite.Taxonomy;
using MyMediaLite.IO;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Abstract class for matrix factorization based item predictors</summary>
	public abstract class MF : Recommender, IIterativeModel
	{
		/// <summary>Latent user factor matrix</summary>
		protected Matrix<float> user_factors;
		/// <summary>Latent item factor matrix</summary>
		protected Matrix<float> item_factors;

		/// <summary>Mean of the normal distribution used to initialize the latent factors</summary>
		public double InitMean { get; set; }

		/// <summary>Standard deviation of the normal distribution used to initialize the latent factors</summary>
		public double InitStdDev { get; set; }

		/// <summary>Number of latent factors per user/item</summary>
		public uint NumFactors { get { return (uint) num_factors; } set { num_factors = (int) value; } }
		/// <summary>Number of latent factors per user/item</summary>
		protected int num_factors = 10;

		/// <summary>Number of iterations over the training data</summary>
		public uint NumIter { get; set; }

		/// <summary>Default constructor</summary>
		public MF()
		{
			NumIter = 30;
			InitMean = 0;
			InitStdDev = 0.1;
		}

		///
		protected virtual void InitModel()
		{
			user_factors = new Matrix<float>(Interactions.MaxUserID + 1, NumFactors);
			item_factors = new Matrix<float>(Interactions.MaxItemID + 1, NumFactors);

			user_factors.InitNormal(InitMean, InitStdDev);
			item_factors.InitNormal(InitMean, InitStdDev);
		}

		///
		public override void Train()
		{
			InitModel();

			for (uint i = 0; i < NumIter; i++)
				Iterate();
		}

		/// <summary>Iterate once over the data</summary>
		public abstract void Iterate();

		///
		public abstract float ComputeObjective();

		/// <summary>Predict the weight for a given user-item combination</summary>
		/// <remarks>
		/// If the user or the item are not known to the recommender, zero is returned.
		/// To avoid this behavior for unknown entities, use CanPredict() to check before.
		/// </remarks>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <returns>the predicted weight</returns>
		public override float Predict(int user_id, int item_id)
		{
			if (user_id >= user_factors.dim1 || item_id >= item_factors.dim1)
				return float.MinValue;

			return DataType.MatrixExtensions.RowScalarProduct(user_factors, user_id, item_factors, item_id);
		}

		///
		public override void SaveModel(string file)
		{
		}

		///
		public override void LoadModel(string file)
		{
		}
	}
}