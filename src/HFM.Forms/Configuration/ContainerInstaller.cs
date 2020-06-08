﻿
using System.Diagnostics.CodeAnalysis;

using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace HFM.Forms.Configuration
{
    [ExcludeFromCodeCoverage]
    public class ContainerInstaller : IWindsorInstaller
    {
        #region IWindsorInstaller Members

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            #region MVP

            // MainPresenter - Singleton
            container.Register(
               Component.For<MainPresenter>(),
               Component.For<IMainView, System.ComponentModel.ISynchronizeInvoke>()
                  .ImplementedBy<MainForm>(),
               Component.For<Models.MainGridModel>(),
               Component.For<Models.UserStatsDataModel>());

            // HistoryPresenterModel - Transient
            container.Register(
                Component.For<HistoryPresenter>()
                    .Named("HistoryPresenter")
                    .LifeStyle.Transient,
                Component.For<IHistoryView>()
                    .ImplementedBy<HistoryForm>()
                    .LifeStyle.Transient);

            container.Register(
               Component.For<IPresenterFactory>()
                  .AsFactory());

            // ProteinCalculatorModel - Transient
            container.Register(
               Component.For<Models.ProteinCalculatorModel>()
                  .LifeStyle.Transient);

            #endregion

            #region View Interfaces

            // Singleton Views
            container.Register(
               Component.For<IMessagesView>()
                  .ImplementedBy<MessagesForm>(),
               Component.For<MessageBoxPresenter>()
                   .Instance(MessageBoxPresenter.Default));

            // Transient Views
            container.Register(
               Component.For<IQueryView>()
                  .ImplementedBy<QueryDialog>()
                     .Named("QueryDialog")
                        .LifeStyle.Transient,
               Component.For<IBenchmarksView>()
                  .ImplementedBy<BenchmarksForm>()
                     .Named("BenchmarksForm")
                        .LifeStyle.Transient,
               Component.For<IPreferencesView>()
                  .ImplementedBy<PreferencesDialog>()
                     .Named("PreferencesDialog")
                        .LifeStyle.Transient,
               Component.For<IProteinCalculatorView>()
                  .ImplementedBy<ProteinCalculatorForm>()
                     .Named("ProteinCalculatorForm")
                        .LifeStyle.Transient,
               Component.For<IViewFactory>()
                  .AsFactory());

            #endregion

            #region Service Interfaces

            // IAutoRun - Singleton
            container.Register(
               Component.For<IAutoRun>()
                  .ImplementedBy<AutoRun>());

            // IExternalProcessStarter - Singleton
            container.Register(
               Component.For<IExternalProcessStarter>()
                  .ImplementedBy<ExternalProcessStarter>());

            // IUpdateLogic - Singleton
            container.Register(
               Component.For<IUpdateLogic>()
                  .ImplementedBy<UpdateLogic>());

            #endregion
        }

        #endregion
    }
}
