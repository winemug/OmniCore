using OmniCore.Model.Eros;
using OmniCore.Model.Interfaces.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using System.ComponentModel;
using OmniCore.Model.Eros.Data;

namespace OmniCore.Mobile.ViewModels.Pod
{
    public class DebugViewModel : BaseViewModel
    {
        private List<ErosMessageExchangeResult> results;
        public List<ErosMessageExchangeResult> Results { get => results; set => SetProperty(ref results, value); }

        private const int MAX_RESULTS = 10;
        public DebugViewModel()
        {
            InitializeResults();
        }

        protected override void OnPodPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        private void InitializeResults()
        {
            Results = new List<ErosMessageExchangeResult>();
            foreach (var result in ErosRepository.Instance.GetHistoricalResultsForDisplay(MAX_RESULTS))
                Results.Add(result);
        }

        private void UpdateResultList()
        {
            var newResults = ErosRepository.Instance.GetHistoricalResultsForDisplay(MAX_RESULTS);
            newResults.Reverse();

            foreach(var newResult in newResults)
            {
                if (!Results.Any(x => x.Id == newResult.Id))
                {
                    Results.Insert(0, newResult);
                }
            }

            for(int i=Results.Count; i>MAX_RESULTS; i--)
            {
                Results.RemoveAt(i - 1);
            }
        }
    }
}
