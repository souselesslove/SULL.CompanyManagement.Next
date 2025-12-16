using System.Text.Json;
using Microsoft.Extensions.Logging;
using SULL.HospitalManagement.IServices;
using VOL.Core.ManageUser;
using VOL.Core.RabbitMQ;
using VOL.Entity.DomainModels;
using Microsoft.Extensions.DependencyInjection;
using VOL.Core.Extensions;

namespace SULL.HospitalManagement.Services.Consumers;

public class TodoConsumer : RabbitConsumerBase
{
    private readonly IServiceProvider _serviceProvider;

    public TodoConsumer(IRabbitMqPersistentConnection connection, ILogger<TodoConsumer> logger, IServiceProvider serviceProvider) : base(connection, logger, Define.TodoQueue)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleMessageAsync(string messageJson, CancellationToken cancellationToken)
    {
        try
        {
            var saveModel = JsonSerializer.Deserialize<SaveModel>(messageJson);
            if (saveModel.MainData.TryGetValue("operate", out object operate) && operate?.ToString() == "complete")
            {
                if (saveModel.MainData.TryGetValue("todoListId", out object todoListIdObj) && long.TryParse(todoListIdObj.ToString(), out long todoListId))
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var repository = scope.ServiceProvider.GetRequiredService<SULL.HospitalManagement.IRepositories.ITodoListRepository>();
                        var todo = repository.FindFirst(x => x.Id == todoListId);
                        if (todo != null)
                        {
                            todo.IsFinished = true;
                            todo.FinishTime = DateTime.Now;
                            todo.FinalStatus = 100;
                            repository.Update(todo);
                            await repository.SaveChangesAsync();
                        }
                    }
                }
            }
            else
            {
                _logger.LogInformation("处理消息：{0}", messageJson);
                // 如果接收人未指定，则需要加入消息发送对象的处理逻辑，目前先全部发给管理员
                if(saveModel.MainData.TryGetValue("LatestAssignUserId", out object receiverObj) && int.TryParse(receiverObj.ToString(), out int receiver) && receiver > 0){
                } else {
                    saveModel.MainData.Add("LatestAssignUserId", 1);
                    saveModel.MainData.Add("LatestAssignUser", "管理员");
                }
                using (var scope = _serviceProvider.CreateScope())
                {
                    var todoListService = scope.ServiceProvider.GetRequiredService<ITodoListService>();
                    todoListService.Add(saveModel);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "处理消息失败");
            await Task.Delay(20000, cancellationToken);
            throw;
        }
    }
}
