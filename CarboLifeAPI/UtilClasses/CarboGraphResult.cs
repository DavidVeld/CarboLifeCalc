using CarboLifeAPI.Data;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CarboLifeAPI
{
    /// <summary>
    /// This class is used to store the information to colour in the model in Revit.
    /// </summary>
    public class CarboGraphResult
    {
        public string ValueName { get; set; }
        public string Unit { get; set; }
        public string ColourLegendName { get; set; }

        /// <summary>
        /// This paramater owns all the data for import to Revit
        /// </summary>
        public IList<CarboValues> entireProjectData;

        /// <summary>
        /// This paramater owns all the visible elements
        /// </summary>
        public IList<CarboValues> selectedData;
        /// <summary>
        /// This paramater owns all the data which lower than the given max
        /// </summary>
        public IList<CarboValues> notSelectedData;


        /// <summary>
        /// This paramater owns all the data thatFits the filter criteria
        /// </summary>
        public IList<CarboValues> validData;

        /// <summary>
        /// This paramater owns all the data which higher than the given max
        /// </summary>
        public IList<CarboValues> outOfBoundsMaxData;

        /// <summary>
        /// This paramater owns all the data which lower than the given max
        /// </summary>
        public IList<CarboValues> outOfBoundsMinData;

        /// <summary>
        /// Elements in the project that are left out of the calculation, so not in any list above.
        /// Carried so the colouring can clear an override left on them from before they were
        /// excluded: it only resets what it is given, and these are no longer in entireProjectData.
        /// </summary>
        public IList<Int64> excludedIds;


        public double max;

        public double min;

        public CarboGraphResult()
        {
            entireProjectData = new List<CarboValues>();
            validData = new List<CarboValues>();
            outOfBoundsMaxData = new List<CarboValues>();
            outOfBoundsMinData = new List<CarboValues>();
            notSelectedData = new List<CarboValues>();
            selectedData = new List<CarboValues>();
            excludedIds = new List<Int64>();
            ColourLegendName = "CLC_ColourLegend";
            Unit = "";
            max = double.PositiveInfinity;
            min = double.NegativeInfinity;
        }

        /// <summary>
        /// This method will filter the non-usable elements from the project;
        /// This is highest level filter and should always create a full filter sequence.
        /// </summary>
        /// <param name="applicablelements">This is a list of elements in the project that needs to be considered, if no elements are selected, the entire project is looked at</param>
        public void FilterNonVisible(List<Int64> applicablelements)
        {
            try
            {
                if (entireProjectData == null || entireProjectData.Count <= 0)
                    throw new Exception("The project has no data, we cannot filter");

                //If the list send is not present or empty then the entire range needs to be considered
                if (applicablelements == null || applicablelements.Count == 0)
                {
                    List<Int64> listOfAllElementIds = entireProjectData.Select(i => i.Id).Distinct().ToList();
                    if (listOfAllElementIds.Count > 0)
                    {
                        applicablelements = listOfAllElementIds;
                    }
                }

                if (Utils.IsEmpty(applicablelements))
                    throw new Exception("Could not collect visible or selected elements from the given dataset");

                //We will now collect the elements with said id:
                //Every list this filter owns is rebuilt, so calling it twice cannot double up the results.
                validData = new List<CarboValues>();
                notSelectedData = new List<CarboValues>();
                selectedData = new List<CarboValues>();

                foreach (CarboValues cv in entireProjectData)
                {
                    bool elementIsVisible = false;
                    //Check if element is visible:
                    elementIsVisible = applicablelements.Contains(cv.Id);

                    if (elementIsVisible == true)
                    {
                        selectedData.Add(cv);
                    }
                    else
                    {
                        notSelectedData.Add(cv);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// This is the second level of data where we filter the min and max out of the list
        /// </summary>
        /// <param name="_minCutoff">The min value</param>
        /// <param name="_maxCutoff">The Maximum Value</param>
        public void FilterMinMax(double minCutoff, double maxCutoff)
        {
            List<CarboValues> bufferListofValidData = new List<CarboValues>();
            outOfBoundsMinData = new List<CarboValues>();
            outOfBoundsMaxData = new List<CarboValues>();

            min = minCutoff;
            max = maxCutoff;

            try
            {
                if (selectedData == null || selectedData.Count <= 0)
                    throw new Exception("The project has no data, we cannot filter");


                foreach(CarboValues cv in selectedData)
                {
                    //A value that is not a number (a zero volume element divided out, for instance) compares
                    //false against everything, so it would otherwise slip through as valid and wreck the scale.
                    if (double.IsNaN(cv.Value))
                        outOfBoundsMaxData.Add(cv);
                    else if (cv.Value < min) // too low
                        outOfBoundsMinData.Add(cv);
                    else if (cv.Value > max) // too high
                        outOfBoundsMaxData.Add(cv);
                    else // Value falls within range: (cv.Value > minCutoff && cv.Value < maxCutoff)
                    {
                        bufferListofValidData.Add(cv);
                    }
                }

                //We can now use the bufferList for the valid Values.
                validData.Clear();
                validData = bufferListofValidData;

            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// The highest value in the data the cutoffs apply to.
        /// </summary>
        /// <remarks>
        /// This reads the set the min/max filter works on, never validData. Reading the already filtered
        /// set would make the range shrink a little further on every pass, so the cutoff sliders could
        /// only ever be narrowed and never widened again.
        /// </remarks>
        public double getMaxValue()
        {
            IList<CarboValues> source = GetUnfilteredData();

            if (source.Count == 0)
                return 9999;

            double result = double.NegativeInfinity;
            foreach (CarboValues cv in source)
            {
                if (double.IsNaN(cv.Value) || double.IsInfinity(cv.Value))
                    continue;
                if (cv.Value > result)
                    result = cv.Value;
            }

            return double.IsNegativeInfinity(result) ? 9999 : result;
        }

        /// <summary>
        /// The lowest value in the data the cutoffs apply to. See <see cref="getMaxValue"/>.
        /// </summary>
        public double getMinValue()
        {
            IList<CarboValues> source = GetUnfilteredData();

            if (source.Count == 0)
                return -99999;

            double result = double.PositiveInfinity;
            foreach (CarboValues cv in source)
            {
                if (double.IsNaN(cv.Value) || double.IsInfinity(cv.Value))
                    continue;
                if (cv.Value < result)
                    result = cv.Value;
            }

            return double.IsPositiveInfinity(result) ? -99999 : result;
        }

        /// <summary>
        /// Returns the value at the given percentile of the unfiltered data, which gives a sensible
        /// starting cutoff for a heavily skewed distribution where a single outlier flattens everything else.
        /// </summary>
        /// <param name="percentile">Where to sample, from 0 to 100.</param>
        public double GetPercentile(double percentile)
        {
            List<double> sorted = new List<double>();
            foreach (CarboValues cv in GetUnfilteredData())
            {
                if (!double.IsNaN(cv.Value) && !double.IsInfinity(cv.Value))
                    sorted.Add(cv.Value);
            }

            if (sorted.Count == 0)
                return 0;

            sorted.Sort();

            if (percentile <= 0) return sorted[0];
            if (percentile >= 100) return sorted[sorted.Count - 1];

            double position = (percentile / 100.0) * (sorted.Count - 1);
            int lower = (int)Math.Floor(position);
            int upper = (int)Math.Ceiling(position);

            if (upper >= sorted.Count) upper = sorted.Count - 1;
            if (lower == upper)
                return sorted[lower];

            return sorted[lower] + (sorted[upper] - sorted[lower]) * (position - lower);
        }

        /// <summary>
        /// The elements the cutoffs are applied to: the visible selection when it has been established,
        /// the whole project otherwise.
        /// </summary>
        private IList<CarboValues> GetUnfilteredData()
        {
            if (selectedData != null && selectedData.Count > 0)
                return selectedData;

            if (entireProjectData != null)
                return entireProjectData;

            return new List<CarboValues>();
        }

        public List<double> GetUniqueValues()
        {
            List<double> thisResult = new List<double>();

            try
            {
                if (validData != null)
                    thisResult = validData.Select(x => Math.Round(x.Value, 3)).Distinct().ToList();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }

            return thisResult;

        }

    }

}
