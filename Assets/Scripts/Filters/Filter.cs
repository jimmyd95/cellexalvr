﻿using System;
using System.Collections.Generic;
using CellexalVR.AnalysisLogic;
using UnityEngine;

namespace CellexalVR.Filters
{
    /// <summary>
    /// Represents a filter that can be used with the selection tool to only select some cells that fulfill some criteria.
    /// </summary>
    public class Filter
    {
        public BooleanExpression.Expr Expression { get; set; }

        /// <summary>
        /// Checks if a cell passes this filter.
        /// </summary>
        /// <param name="cell">The cell to check.</param>
        /// <returns>True if the cell passed this filter, false otherwise.</returns>
        /// 
        public bool Pass(Cell cell)
        {
            return Expression.Eval(cell);
        }

        /// <summary>
        /// Gets all the genes in this filter.
        /// </summary>
        /// <param name="onlyPercent">True if only genes that have not been converted from percent expressions should be returned.</param>
        public List<string> GetGenes(bool onlyPercent = false)
        {
            List<string> result = new List<string>();
            Expression.GetGenes(ref result, onlyPercent);
            return result;
        }

        /// <summary>
        /// Gets all the facs measurements in this filter.
        /// </summary>
        /// <param name="onlyPercent">True if only facs measurements that have not been converted from percent expressions should be returned.</param>
        public List<string> GetFacs(bool onlyPercent = false)
        {
            List<string> result = new List<string>();
            Expression.GetFacs(ref result, onlyPercent);
            return result;
        }

        /// <summary>
        /// Gets all the attributes in this filter.
        /// </summary>
        /// <param name="onlyPercent">True if only attributes that have not been converted from percent expressions should be returned.</param>
        public List<string> GetAttributes()
        {
            List<string> result = new List<string>();
            Expression.GetAttributes(ref result);
            return result;
        }

        /// <summary>
        /// Gets all the numerical attributes in this filter.
        /// </summary>
        /// <param name="onlyPercent">True if only attributes that have not been converted from percent expressions should be returned.</param>
        public List<string> GetNumericalAttributes()
        {
            List<string> result = new List<string>();
            Expression.GetNumericalAttributes(ref result);
            return result;
        }

    }
}