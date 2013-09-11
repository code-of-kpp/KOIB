using System; 

using System.IO; 

using Croc.Workflow.ComponentModel; 

 

 

namespace Croc.Workflow.Runtime.Hosting 

{ 

    /// <summary> 

    /// Сервис постоянства, который сохраняет состояние в файл 

    /// </summary> 

    public class FileWorkflowPersistenceService : WorkflowPersistenceService 

    { 

        /// <summary> 

        /// Путь к директории, в которой будем складывать файлы с сохраненным состоянием 

        /// </summary> 

        private string _dirForSavePath; 

 

 

        public FileWorkflowPersistenceService(string dirForSavePath) 

        { 

            CodeContract.Requires(!string.IsNullOrEmpty(dirForSavePath)); 

 

 

            if (!Directory.Exists(dirForSavePath)) 

                throw new ArgumentException(string.Format( 

                    "Директория '{0}' не найдена", dirForSavePath), "dirForSavePath"); 

 

 

            _dirForSavePath = dirForSavePath; 

        } 

 

 

        private string GetFileName(Guid instanceId) 

        { 

            return Path.Combine(_dirForSavePath, string.Format("{0}.wfs", instanceId)); 

        } 

 

 

        public override WorkflowExecutionContext LoadWorkflowInstanceState(Guid instanceId) 

        { 

            var fileName = GetFileName(instanceId); 

 

 

            if (!File.Exists(fileName)) 

                throw new Exception(string.Format( 

                    "Состояние экземпляра потока работ с идентификатором {0} не было сохранено", instanceId)); 

 

 

            try 

            { 


                using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None)) 

                { 

                    return WorkflowExecutionContext.Load(stream); 

                } 

            } 

            catch (Exception ex) 

            { 

                throw new Exception(string.Format( 

                    "Ошибка загрузки состояния экземпляра потока работ с идентификатором {0}", instanceId), ex); 

            } 

        } 

 

 

        public override void SaveWorkflowInstanceState(WorkflowExecutionContext context) 

        { 

            try 

            { 

                var fileName = GetFileName(context.InstanceId); 

                using (var stream = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) 

                { 

                    WorkflowExecutionContext.Save(context, stream); 

                } 

            } 

            catch (Exception ex) 

            { 

                throw new Exception(string.Format( 

                    "Ошибка сохранения состояния экземпляра потока работ с идентификатором {0}",  

                    context.InstanceId), ex); 

            } 

        } 

    } 

}


