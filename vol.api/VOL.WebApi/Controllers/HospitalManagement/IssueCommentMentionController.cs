/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹IssueCommentMentionController编写
 */
using Microsoft.AspNetCore.Mvc;
using VOL.Core.Controllers.Basic;
using VOL.Entity.AttributeManager;
using SULL.HospitalManagement.IServices;
namespace SULL.HospitalManagement.Controllers
{
    [Route("api/ssbh/IssueCommentMention")]
    [PermissionTable(Name = "IssueCommentMention")]
    public partial class IssueCommentMentionController : ApiBaseController<IIssueCommentMentionService>
    {
    public IssueCommentMentionController(IIssueCommentMentionService service)
    : base(service)
    {
    }
    }
    }

