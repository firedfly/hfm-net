﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using HFM.Core.Client;
using HFM.Core.WorkUnits;
using HFM.Preferences;
using HFM.Proteins;

namespace HFM.Forms.Models
{
    public class BenchmarksModel : ViewModelBase, IBenchmarksReportSource
    {
        public IPreferenceSet Preferences { get; }
        public IProteinService ProteinService { get; }
        public IProteinBenchmarkService BenchmarkService { get; }
        public IEnumerable<BenchmarksReport> Reports { get; }

        public BenchmarksModel(IPreferenceSet preferences, IProteinService proteinService,
            IProteinBenchmarkService benchmarkService, IEnumerable<BenchmarksReport> reports)
        {
            Preferences = preferences ?? new InMemoryPreferenceSet();
            ProteinService = proteinService ?? NullProteinService.Instance;
            BenchmarkService = benchmarkService ?? NullProteinBenchmarkService.Instance;
            Reports = reports ?? Array.Empty<BenchmarksReport>();

            SlotIdentifiers = new BindingSource();
            SlotIdentifiers.DataSource = new BindingList<ListItem> { RaiseListChangedEvents = false };
            SlotProjects = new BindingSource();
            SlotProjects.DataSource = new BindingList<ListItem> { RaiseListChangedEvents = false };
            SelectedSlotProjectListItems = new ListItemCollection();
        }

        public override void Load()
        {
            FormLocation = Preferences.Get<Point>(Preference.BenchmarksFormLocation);
            FormSize = Preferences.Get<Size>(Preference.BenchmarksFormSize);
            GraphColors.Clear();
            GraphColors.AddRange(Preferences.Get<List<Color>>(Preference.GraphColors) ?? new List<Color>());

            RefreshSlotIdentifiers();
            SetFirstSlotIdentifier();
        }

        public override void Save()
        {
            Preferences.Set(Preference.BenchmarksFormLocation, FormLocation);
            Preferences.Set(Preference.BenchmarksFormSize, FormSize);
            Preferences.Set(Preference.GraphColors, GraphColors);
            Preferences.Save();
        }

        public Point FormLocation { get; set; }

        public Size FormSize { get; set; }

        IReadOnlyList<Color> IBenchmarksReportSource.Colors => GraphColors;

        public List<Color> GraphColors { get; } = new List<Color>();

        public BindingSource SlotIdentifiers { get; }

        public BindingSource SlotProjects { get; }

        #region Protein

        private void SetProtein()
        {
            var projectID = SelectedSlotProject?.Value;
            if (projectID is null)
            {
                Protein = null;
                return;
            }

            Protein = ProteinService.Get(projectID.Value);
        }

        private Protein _protein;

        public Protein Protein
        {
            get => _protein;
            private set
            {
                if (_protein != value)
                {
                    _protein = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(WorkUnitName));
                    OnPropertyChanged(nameof(Credit));
                    OnPropertyChanged(nameof(KFactor));
                    OnPropertyChanged(nameof(Frames));
                    OnPropertyChanged(nameof(NumberOfAtoms));
                    OnPropertyChanged(nameof(Core));
                    OnPropertyChanged(nameof(DescriptionUrl));
                    OnPropertyChanged(nameof(PreferredDays));
                    OnPropertyChanged(nameof(MaximumDays));
                    OnPropertyChanged(nameof(Contact));
                    OnPropertyChanged(nameof(ServerIP));
                }
            }
        }

        public string WorkUnitName => Protein?.WorkUnitName;

        public double? Credit => Protein?.Credit;

        public double? KFactor => Protein?.KFactor;

        public int? Frames => Protein?.Frames;

        public int? NumberOfAtoms => Protein?.NumberOfAtoms;

        public string Core => Protein?.Core;

        public string DescriptionUrl => Protein?.Description;

        public double? PreferredDays => Protein?.PreferredDays;

        public double? MaximumDays => Protein?.MaximumDays;

        public string Contact => Protein?.Contact;

        public string ServerIP => Protein?.ServerIP;

        #endregion

        // SelectedSlotIdentifier
        SlotIdentifier? IBenchmarksReportSource.SlotIdentifier => SelectedSlotIdentifier?.Value;

        private ValueItem<SlotIdentifier> _selectedSlotIdentifier;

        public ValueItem<SlotIdentifier> SelectedSlotIdentifier
        {
            get => _selectedSlotIdentifier;
            set
            {
                if (_selectedSlotIdentifier != value)
                {
                    _selectedSlotIdentifier = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedSlotDeleteEnabled));

                    RefreshSlotProjects();
                    SetFirstSlotProject();
                }
            }
        }

        public bool SelectedSlotDeleteEnabled => SelectedSlotIdentifier != null && SelectedSlotIdentifier.Value != SlotIdentifier.AllSlots;

        private void RefreshSlotIdentifiers()
        {
            var slots = Enumerable.Repeat(SlotIdentifier.AllSlots, 1)
                .Concat(BenchmarkService.GetSlotIdentifiers().OrderBy(x => x.Name))
                .Select(x => new ListItem(x.ToString(), new ValueItem<SlotIdentifier>(x)));

            SlotIdentifiers.Clear();
            foreach (var slot in slots)
            {
                SlotIdentifiers.Add(slot);
            }
            SlotIdentifiers.ResetBindings(false);
        }

        internal IEnumerable<ValueItem<SlotIdentifier>> SlotIdentifierValueItems =>
            SlotIdentifiers.Cast<ListItem>().Select(x => x.GetValue<ValueItem<SlotIdentifier>>());

        private void SetFirstSlotIdentifier()
        {
            SelectedSlotIdentifier = SlotIdentifierValueItems.FirstOrDefault();
        }

        // SelectedSlotProject
        IReadOnlyCollection<int> IBenchmarksReportSource.Projects =>
            SelectedSlotProjectListItems.Select(x => x.GetValue<ValueItem<int>>().Value).ToList();

        public ValueItem<int> SelectedSlotProject { get; private set; }

        private ListItemCollection _selectedSlotProjectListItems;

        public ListItemCollection SelectedSlotProjectListItems
        {
            get => _selectedSlotProjectListItems;
            set
            {
                if (value != null)
                {
                    if (_selectedSlotProjectListItems != null)
                    {
                        _selectedSlotProjectListItems.CollectionChanged -= OnSelectedSlotProjectListItemsChanged;
                    }
                    _selectedSlotProjectListItems = value;
                    _selectedSlotProjectListItems.CollectionChanged += OnSelectedSlotProjectListItemsChanged;
                }
            }
        }

        protected virtual void OnSelectedSlotProjectListItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (SelectedSlotProjectListItems.Count > 0)
            {
                SelectedSlotProject = SelectedSlotProjectListItems.First().GetValue<ValueItem<int>>();
                RunReports();
            }
            else
            {
                SelectedSlotProject = null;
                ClearReports();
            }
            SetProtein();
        }

        private void RefreshSlotProjects()
        {
            SlotProjects.Clear();

            if (SelectedSlotIdentifier != null)
            {
                var projects = BenchmarkService.GetBenchmarkProjects(SelectedSlotIdentifier.Value)
                    .Select(x => new ListItem(x.ToString(), new ValueItem<int>(x)));

                foreach (var project in projects)
                {
                    SlotProjects.Add(project);
                }
            }

            SlotProjects.ResetBindings(false);
        }

        internal IEnumerable<ListItem> SlotProjectListItems => SlotProjects.Cast<ListItem>();

        private void SetFirstSlotProject()
        {
            if (SelectedSlotProjectListItems.Count > 0)
            {
                SelectedSlotProjectListItems.Clear();
            }
            var item = SlotProjectListItems.FirstOrDefault();
            if (!item.IsEmpty)
            {
                SelectedSlotProjectListItems.Add(item);
            }
        }

        /// <summary>
        /// Set before calling <see cref="Load"/> to default the selected project to this project ID.
        /// </summary>
        public int DefaultProjectID { get; set; }

        public void SetDefaultSlotProject()
        {
            var defaultSlotProject = SlotProjectListItems.FirstOrDefault(x => x.GetValue<ValueItem<int>>().Value == DefaultProjectID);
            if (!defaultSlotProject.IsEmpty)
            {
                if (SelectedSlotProjectListItems.Count > 0)
                {
                    SelectedSlotProjectListItems.Clear();
                }
                SelectedSlotProjectListItems.Add(defaultSlotProject);
            }
        }

        // Reports
        public void RunReports()
        {
            foreach (var report in Reports)
            {
                report.Generate(this);
                switch (report.Key)
                {
                    case TextBenchmarksReport.KeyName:
                        BenchmarkText = (IReadOnlyCollection<string>)report.Result;
                        break;
                    case FrameTimeZedGraphBenchmarksReport.KeyName:
                        FrameTimeGraphControl = (Control)report.Result;
                        break;
                    case ProductionZedGraphBenchmarksReport.KeyName:
                        ProductionGraphControl = (Control)report.Result;
                        break;
                    case ProjectComparisonZedGraphBenchmarksReport.KeyName:
                        ProjectComparisonGraphControl = (Control)report.Result;
                        break;
                }
            }
        }

        private void ClearReports()
        {
            BenchmarkText = null;
            FrameTimeGraphControl = null;
            ProductionGraphControl = null;
            ProjectComparisonGraphControl = null;
        }

        private IReadOnlyCollection<string> _benchmarkText;

        public IReadOnlyCollection<string> BenchmarkText
        {
            get => _benchmarkText;
            private set
            {
                if (_benchmarkText != value)
                {
                    _benchmarkText = value;
                    OnPropertyChanged();
                }
            }
        }

        private Control _frameTimeGraphControl;

        public Control FrameTimeGraphControl
        {
            get => _frameTimeGraphControl;
            set
            {
                if (_frameTimeGraphControl != value)
                {
                    _frameTimeGraphControl = value;
                    OnPropertyChanged();
                }
            }
        }

        private Control _productionGraphControl;

        public Control ProductionGraphControl
        {
            get => _productionGraphControl;
            set
            {
                if (_productionGraphControl != value)
                {
                    _productionGraphControl = value;
                    OnPropertyChanged();
                }
            }
        }

        private Control _projectComparisonGraphControl;

        public Control ProjectComparisonGraphControl
        {
            get => _projectComparisonGraphControl;
            set
            {
                if (_projectComparisonGraphControl != value)
                {
                    _projectComparisonGraphControl = value;
                    OnPropertyChanged();
                }
            }
        }

        // Benchmark Actions
        public void RemoveSlot(SlotIdentifier slotIdentifier)
        {
            BenchmarkService.RemoveAll(slotIdentifier);
            RefreshSlotIdentifiers();
        }

        public void RemoveProject(SlotIdentifier slotIdentifier, int projectID)
        {
            BenchmarkService.RemoveAll(slotIdentifier, projectID);
            RefreshSlotIdentifiers();
            RefreshSlotProjects();
        }
    }
}
