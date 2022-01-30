﻿using HFM.Core.Client;
using HFM.Core.WorkUnits;
using HFM.Log;
using HFM.Proteins;

using NUnit.Framework;

namespace HFM.Core.Data
{
    [Ignore("Hanging on CI build")]
    [TestFixture]
    public class WorkUnitContextRepositoryTests
    {
        [TestFixture]
        public class WhenInsertingNewWorkUnit : WorkUnitContextRepositoryTests
        {
            private ArtifactFolder _artifacts;
            private string _connectionString;
            private readonly Guid _clientGuid = Guid.NewGuid();
            private IWorkUnitRepository _repository;
            private readonly DateTime _assigned = DateTime.UtcNow;
            private bool _insertResult;

            [SetUp]
            public void BeforeEach()
            {
                _artifacts = new ArtifactFolder();
                _connectionString = $"Data Source={_artifacts.GetRandomFilePath()}";
                _repository = new TestableWorkUnitContextRepository(_connectionString);

                var settings = new ClientSettings
                {
                    Name = "GTX3090",
                    Server = "gtx3090.awesome.com",
                    Port = ClientSettings.DefaultPort,
                    Guid = _clientGuid
                };
                var workUnit = new WorkUnit
                {
                    FoldingID = "harlam357",
                    Team = 32,
                    CoreVersion = new Version("0.0.18"),
                    ProjectID = 1,
                    ProjectRun = 2,
                    ProjectClone = 3,
                    ProjectGen = 4,
                    UnitResult = WorkUnitResult.FinishedUnit,
                    Assigned = _assigned,
                    Finished = _assigned.AddHours(6),
                    FramesObserved = 1,
                    Frames = new Dictionary<int, LogLineFrameData>
                    {
                        {
                            99, new LogLineFrameData
                            {
                                ID = 99,
                                RawFramesComplete = 990_000,
                                RawFramesTotal = 1_000_000,
                                TimeStamp = TimeSpan.FromSeconds(10),
                                Duration = TimeSpan.Zero
                            }
                        },
                        {
                            100, new LogLineFrameData
                            {
                                ID = 100,
                                RawFramesComplete = 1_000_000,
                                RawFramesTotal = 1_000_000,
                                TimeStamp = TimeSpan.FromSeconds(38),
                                Duration = TimeSpan.FromSeconds(28)
                            }
                        }
                    }
                };
                var protein = new Protein
                {
                    ProjectNumber = 1,
                    NumberOfAtoms = 350000,
                    PreferredDays = 1.0,
                    MaximumDays = 3.0,
                    Credit = 25000.0,
                    Frames = 100,
                    Core = "GRO_A8",
                    KFactor = 0.3
                };

                var workUnitModel = CreateWorkUnitModel(settings, workUnit, protein, 1);
                _insertResult = _repository.Insert(workUnitModel);
            }

            [TearDown]
            public void AfterEach() => _artifacts?.Dispose();

            [Test]
            public void ThenNewClientIsInserted()
            {
                Assert.IsTrue(_insertResult);

                using var context = new WorkUnitContext(_connectionString);
                var client = context.Clients.First();
                Assert.AreNotEqual(0, client.ID);
                Assert.AreEqual("GTX3090", client.Name);
                Assert.AreEqual("gtx3090.awesome.com:36330", client.ConnectionString);
                Assert.AreEqual(_clientGuid.ToString(), client.Guid);
            }

            [Test]
            public void ThenNewProteinIsInserted()
            {
                Assert.IsTrue(_insertResult);

                using var context = new WorkUnitContext(_connectionString);
                var protein = context.Proteins.First();
                Assert.AreNotEqual(0, protein.ID);
                Assert.AreEqual(1, protein.ProjectID);
                Assert.AreEqual(25000.0, protein.Credit);
                Assert.AreEqual(0.3, protein.KFactor);
                Assert.AreEqual(100, protein.Frames);
                Assert.AreEqual("GRO_A8", protein.Core);
                Assert.AreEqual(350000, protein.Atoms);
                Assert.AreEqual(1.0, protein.TimeoutDays);
                Assert.AreEqual(3.0, protein.ExpirationDays);
            }

            [Test]
            public void ThenNewWorkUnitIsInserted()
            {
                Assert.IsTrue(_insertResult);

                using var context = new WorkUnitContext(_connectionString);
                var workUnit = context.WorkUnits.First();
                Assert.AreNotEqual(0, workUnit.ID);
                Assert.AreEqual("harlam357", workUnit.DonorName);
                Assert.AreEqual(32, workUnit.DonorTeam);
                Assert.AreEqual("0.0.18", workUnit.CoreVersion);
                Assert.AreEqual(WorkUnitResultString.FinishedUnit, workUnit.Result);
                Assert.AreEqual(_assigned, workUnit.Assigned);
                Assert.AreEqual(_assigned.AddHours(6), workUnit.Finished);
                Assert.AreEqual(2, workUnit.ProjectRun);
                Assert.AreEqual(3, workUnit.ProjectClone);
                Assert.AreEqual(4, workUnit.ProjectGen);
                Assert.AreEqual(null, workUnit.HexID);
                Assert.AreEqual(100, workUnit.FramesCompleted);
                Assert.AreEqual(28, workUnit.FrameTimeInSeconds);
                Assert.AreEqual(context.Proteins.First().ID, workUnit.ProteinID);
                Assert.AreEqual(context.Clients.First().ID, workUnit.ClientID);
                Assert.AreEqual(1, workUnit.ClientSlot);
            }

            [Test]
            public void ThenNewWorkUnitFramesAreInserted()
            {
                Assert.IsTrue(_insertResult);

                using var context = new WorkUnitContext(_connectionString);
                long workUnitID = context.WorkUnits.First().ID;
                var frames = context.WorkUnitFrames.Where(x => x.WorkUnitID == workUnitID).ToDictionary(x => x.FrameID);

                var f = frames[99];
                Assert.AreEqual(99, f.FrameID);
                Assert.AreEqual(990_000, f.RawFramesComplete);
                Assert.AreEqual(1_000_000, f.RawFramesTotal);
                Assert.AreEqual(TimeSpan.FromSeconds(10), f.TimeStamp);
                Assert.AreEqual(TimeSpan.Zero, f.Duration);

                f = frames[100];
                Assert.AreEqual(100, f.FrameID);
                Assert.AreEqual(1_000_000, f.RawFramesComplete);
                Assert.AreEqual(1_000_000, f.RawFramesTotal);
                Assert.AreEqual(TimeSpan.FromSeconds(38), f.TimeStamp);
                Assert.AreEqual(TimeSpan.FromSeconds(28), f.Duration);
            }
        }

        [TestFixture]
        public class WhenInsertingExistingWorkUnit : WorkUnitContextRepositoryTests
        {
            private ArtifactFolder _artifacts;
            private string _connectionString;
            private IWorkUnitRepository _repository;
            private readonly DateTime _assigned = DateTime.UtcNow;
            private bool _insertResult;

            [SetUp]
            public void BeforeEach()
            {
                _artifacts = new ArtifactFolder();
                _connectionString = $"Data Source={_artifacts.GetRandomFilePath()}";
                _repository = new TestableWorkUnitContextRepository(_connectionString);

                var settings = new ClientSettings();
                var workUnit = new WorkUnit
                {
                    ProjectID = 1,
                    ProjectRun = 2,
                    ProjectClone = 3,
                    ProjectGen = 4,
                    Assigned = _assigned,
                    Finished = _assigned.AddHours(6)
                };
                var protein = new Protein
                {
                    ProjectNumber = 1
                };

                var workUnitModel = CreateWorkUnitModel(settings, workUnit, protein);
                _repository.Insert(workUnitModel);
                _insertResult = _repository.Insert(workUnitModel);
            }

            [TearDown]
            public void AfterEach() => _artifacts?.Dispose();

            [Test]
            public void ThenInsertReturnsFalse() => Assert.IsFalse(_insertResult);
        }

        [TestFixture]
        public class WhenInsertingInvalidWorkUnit : WorkUnitContextRepositoryTests
        {
            private ArtifactFolder _artifacts;
            private string _connectionString;
            private IWorkUnitRepository _repository;
            private bool _insertResult;

            [SetUp]
            public void BeforeEach()
            {
                _artifacts = new ArtifactFolder();
                _connectionString = $"Data Source={_artifacts.GetRandomFilePath()}";
                _repository = new TestableWorkUnitContextRepository(_connectionString);

                var settings = new ClientSettings();
                var workUnit = new WorkUnit();
                var protein = new Protein();

                var workUnitModel = CreateWorkUnitModel(settings, workUnit, protein);
                _insertResult = _repository.Insert(workUnitModel);
            }

            [TearDown]
            public void AfterEach() => _artifacts?.Dispose();

            [Test]
            public void ThenInsertReturnsFalse() => Assert.IsFalse(_insertResult);
        }

        [TestFixture]
        public class WhenInsertingWithExistingClientGuid : WorkUnitContextRepositoryTests
        {
            private ArtifactFolder _artifacts;
            private string _connectionString;
            private readonly Guid _clientGuid = Guid.NewGuid();
            private IWorkUnitRepository _repository;
            private readonly DateTime _assigned = DateTime.UtcNow;
            private bool _insertResult;

            [SetUp]
            public void BeforeEach()
            {
                _artifacts = new ArtifactFolder();
                _connectionString = $"Data Source={_artifacts.GetRandomFilePath()}";
                _repository = new TestableWorkUnitContextRepository(_connectionString);

                var settings = new ClientSettings
                {
                    Name = "GTX3090",
                    Server = "gtx3090.awesome.com",
                    Port = ClientSettings.DefaultPort,
                    Guid = _clientGuid
                };
                var workUnit = new WorkUnit
                {
                    ProjectID = 1,
                    ProjectRun = 2,
                    ProjectClone = 3,
                    ProjectGen = 4,
                    Assigned = _assigned,
                    Finished = _assigned.AddHours(6)
                };
                var protein = new Protein
                {
                    ProjectNumber = 1
                };

                var workUnitModel = CreateWorkUnitModel(settings, workUnit, protein);
                _repository.Insert(workUnitModel);

                workUnit.Assigned = _assigned.AddHours(24);
                workUnit.Finished = _assigned.AddHours(30);

                workUnitModel = CreateWorkUnitModel(settings, workUnit, protein);
                _insertResult = _repository.Insert(workUnitModel);
            }

            [TearDown]
            public void AfterEach() => _artifacts?.Dispose();

            [Test]
            public void ThenExistingClientIsReferenced()
            {
                Assert.IsTrue(_insertResult);

                using var context = new WorkUnitContext(_connectionString);
                Assert.AreEqual(1, context.Clients.Count());
                Assert.AreEqual(2, context.WorkUnits.Count());

                var workUnit = context.WorkUnits.OrderByDescending(x => x.ID).First();
                Assert.AreEqual(context.Clients.First().ID, workUnit.ClientID);
            }
        }

        [TestFixture]
        public class WhenInsertingWithExistingClientGuidAndNameOrConnectionChanged : WorkUnitContextRepositoryTests
        {
            private ArtifactFolder _artifacts;
            private string _connectionString;
            private readonly Guid _clientGuid = Guid.NewGuid();
            private IWorkUnitRepository _repository;
            private readonly DateTime _assigned = DateTime.UtcNow;
            private bool _insertResult;

            [SetUp]
            public void BeforeEach()
            {
                _artifacts = new ArtifactFolder();
                _connectionString = $"Data Source={_artifacts.GetRandomFilePath()}";
                _repository = new TestableWorkUnitContextRepository(_connectionString);

                var settings = new ClientSettings
                {
                    Name = "GTX3090",
                    Server = "gtx3090.awesome.com",
                    Port = ClientSettings.DefaultPort,
                    Guid = _clientGuid
                };
                var workUnit = new WorkUnit
                {
                    ProjectID = 1,
                    ProjectRun = 2,
                    ProjectClone = 3,
                    ProjectGen = 4,
                    Assigned = _assigned,
                    Finished = _assigned.AddHours(6)
                };
                var protein = new Protein
                {
                    ProjectNumber = 1
                };

                var workUnitModel = CreateWorkUnitModel(settings, workUnit, protein);
                _repository.Insert(workUnitModel);

                settings.Name = "GTX4080";
                settings.Server = "gtx4080.awesome.com";

                workUnit.Assigned = _assigned.AddHours(24);
                workUnit.Finished = _assigned.AddHours(30);

                workUnitModel = CreateWorkUnitModel(settings, workUnit, protein);
                _insertResult = _repository.Insert(workUnitModel);
            }

            [TearDown]
            public void AfterEach() => _artifacts?.Dispose();

            [Test]
            public void ThenNewClientIsInserted()
            {
                Assert.IsTrue(_insertResult);

                using var context = new WorkUnitContext(_connectionString);
                Assert.AreEqual(2, context.Clients.Count());
                Assert.AreEqual(2, context.WorkUnits.Count());

                var client = context.Clients.OrderByDescending(x => x.ID).First();
                Assert.AreNotEqual(0, client.ID);
                Assert.AreEqual("GTX4080", client.Name);
                Assert.AreEqual("gtx4080.awesome.com:36330", client.ConnectionString);
                Assert.AreEqual(_clientGuid.ToString(), client.Guid);
            }
        }

        [TestFixture]
        public class WhenInsertingWithExistingClientNameAndConnection : WorkUnitContextRepositoryTests
        {
            private ArtifactFolder _artifacts;
            private string _connectionString;
            private readonly Guid _clientGuid = Guid.NewGuid();
            private IWorkUnitRepository _repository;
            private readonly DateTime _assigned = DateTime.UtcNow;
            private bool _insertResult;

            [SetUp]
            public void BeforeEach()
            {
                _artifacts = new ArtifactFolder();
                _connectionString = $"Data Source={_artifacts.GetRandomFilePath()}";
                _repository = new TestableWorkUnitContextRepository(_connectionString);

                var settings = new ClientSettings
                {
                    Name = "GTX3090",
                    Server = "gtx3090.awesome.com",
                    Port = ClientSettings.DefaultPort
                };
                var workUnit = new WorkUnit
                {
                    ProjectID = 1,
                    ProjectRun = 2,
                    ProjectClone = 3,
                    ProjectGen = 4,
                    Assigned = _assigned,
                    Finished = _assigned.AddHours(6)
                };
                var protein = new Protein
                {
                    ProjectNumber = 1
                };

                var workUnitModel = CreateWorkUnitModel(settings, workUnit, protein);
                _repository.Insert(workUnitModel);

                settings.Guid = _clientGuid;

                workUnit.Assigned = _assigned.AddHours(24);
                workUnit.Finished = _assigned.AddHours(30);

                workUnitModel = CreateWorkUnitModel(settings, workUnit, protein);
                _insertResult = _repository.Insert(workUnitModel);
            }

            [TearDown]
            public void AfterEach() => _artifacts?.Dispose();

            [Test]
            public void ThenExistingClientIsReferencedAndUpdatedWithGuidValue()
            {
                Assert.IsTrue(_insertResult);

                using var context = new WorkUnitContext(_connectionString);
                Assert.AreEqual(1, context.Clients.Count());
                Assert.AreEqual(2, context.WorkUnits.Count());

                var workUnit = context.WorkUnits.OrderByDescending(x => x.ID).First();
                var client = context.Clients.First();
                Assert.AreEqual(client.ID, workUnit.ClientID);
                Assert.AreEqual(client.Guid, _clientGuid.ToString());
            }
        }

        [TestFixture]
        public class GivenFinishedAndFailedWorkUnits : WorkUnitContextRepositoryTests
        {
            private ArtifactFolder _artifacts;
            private string _connectionString;
            private readonly Guid _clientGuid = Guid.NewGuid();
            private IWorkUnitRepository _repository;
            private readonly DateTime _utcNow = DateTime.UtcNow;

            [SetUp]
            public void BeforeEach()
            {
                _artifacts = new ArtifactFolder();
                _connectionString = $"Data Source={_artifacts.GetRandomFilePath()}";
                _repository = new TestableWorkUnitContextRepository(_connectionString);

                var settings = new ClientSettings
                {
                    Name = "GTX3090",
                    Server = "gtx3090.awesome.com",
                    Port = ClientSettings.DefaultPort,
                    Guid = _clientGuid
                };
                InsertFinishedAndFailedAssignedAt(_utcNow, settings);
                InsertFinishedAndFailedAssignedAt(new DateTime(2020, 1, 1), settings);
            }

            private void InsertFinishedAndFailedAssignedAt(DateTime assigned, ClientSettings settings)
            {
                var protein = new Protein();
                var finishedWorkUnit = new WorkUnit
                {
                    ProjectID = 1,
                    ProjectRun = 1,
                    ProjectClone = 1,
                    ProjectGen = 1,
                    Assigned = assigned,
                    Finished = assigned.AddHours(6),
                    UnitResult = WorkUnitResult.FinishedUnit
                };
                assigned = assigned.AddHours(6);
                var failedWorkUnit = new WorkUnit
                {
                    ProjectID = 1,
                    ProjectRun = 1,
                    ProjectClone = 1,
                    ProjectGen = 1,
                    Assigned = assigned,
                    Finished = assigned.AddHours(6),
                    UnitResult = WorkUnitResult.BadWorkUnit
                };

                var workUnitModel = CreateWorkUnitModel(settings, finishedWorkUnit, protein, 1);
                _repository.Insert(workUnitModel);
                workUnitModel = CreateWorkUnitModel(settings, failedWorkUnit, protein, 1);
                _repository.Insert(workUnitModel);
            }

            [TearDown]
            public void AfterEach() => _artifacts?.Dispose();

            [Test]
            public void WhenClientStartTimeIsNullThenRepositoryReturnsAllFinishedAndFailedCounts()
            {
                long finished = _repository.CountCompleted("GTX3090 Slot 01", null);
                long failed = _repository.CountFailed("GTX3090 Slot 01", null);
                Assert.AreEqual(2, finished);
                Assert.AreEqual(2, failed);
            }

            [Test]
            public void WhenClientStartTimeIsNotNullThenRepositoryReturnsCountsFinishedAfterDateTime()
            {
                long finished = _repository.CountCompleted("GTX3090 Slot 01", _utcNow);
                long failed = _repository.CountFailed("GTX3090 Slot 01", _utcNow);
                Assert.AreEqual(1, finished);
                Assert.AreEqual(1, failed);
            }
        }

        private static WorkUnitModel CreateWorkUnitModel(ClientSettings settings, WorkUnit workUnit, Protein protein, int slotID = SlotIdentifier.NoSlotID)
        {
            var slotModel = new SlotModel(new NullClient { Settings = settings }) { SlotID = slotID };
            var workUnitModel = new WorkUnitModel(slotModel, workUnit);
            workUnitModel.CurrentProtein = protein;
            return workUnitModel;
        }
    }

    public class TestableWorkUnitContextRepository : WorkUnitContextRepository
    {
        private readonly string _connectionString;

        public TestableWorkUnitContextRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override WorkUnitContext CreateWorkUnitContext()
        {
            var context = new WorkUnitContext(_connectionString);
            context.Database.EnsureCreated();
            return context;
        }
    }
}