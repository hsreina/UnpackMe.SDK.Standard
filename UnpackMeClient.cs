using System;
using System.Collections.Generic;
using System.IO;
using UnpackMe.SDK.Core.Exceptions;
using UnpackMe.SDK.Core.Models;
using UnpackMe.SDK.Core.Request;

namespace UnpackMe.SDK.Core
{
    public class UnpackMeClient : IDisposable
    {
        private RequestHandler _requesthandler;

        public UnpackMeClient(string serviceUrl, string basicLogin = null, string basicPassword = null)
        {
            _requesthandler = new RequestHandler(serviceUrl, basicLogin, basicPassword);
        }

        public void Authenticate(string login, string password)
        {
            LoginResultModel loginResult = _requesthandler.Post<LoginResultModel>("/auth", new[]
            {
                new KeyValuePair<string, string>("login", login),
                new KeyValuePair<string, string>("password", password)
            });

            if (null == loginResult || String.IsNullOrEmpty(loginResult.Token))
            {
                throw new InvalidLoginException();
            }

            _requesthandler.SetToken(loginResult.Token);
        }

        public CommandModel[] GetAvailableCommands()
        {
            return _requesthandler.Get<CommandModel[]>("/command/available");
        }

        public string CreateTaskFromCommandId(string commandId, Stream fileData)
        {
            var path = String.Format("/task/create/{0}", commandId);
            var createTaskResult = 
                _requesthandler.PostStream<CreateTaskResultModel>(path, fileData);

            if (null == createTaskResult || String.IsNullOrEmpty(createTaskResult.TaskId))
            {
                throw new UnpackMeException();
            }

            return createTaskResult.TaskId;
        }

        public TaskModel GetTaskById(string taskid)
        {
            return _requesthandler.Get<TaskModel>(String.Format("/task/{0}", taskid));
        }

        public void SaveTaskFileTo(string taskId, string filename)
        {
            _requesthandler.GetIntoFile(String.Format("/task/{0}/download", taskId), filename);
        }

        public void Dispose()
        {
            _requesthandler.Dispose();
        }
    }
}
