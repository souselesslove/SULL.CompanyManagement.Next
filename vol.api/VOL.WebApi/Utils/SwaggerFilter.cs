using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VOL.WebApi.Utils;

public class SwaggerFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Key为url的相对路径，Value为命名空间名称
        var banList = swaggerDoc.Paths
             .Where(x => !x.Key.ToLower().StartsWith("/api/ssbh"))
             .ToList(); 
        banList.ForEach(x => { swaggerDoc.Paths.Remove(x.Key); });

        //添加对应的控制器描述
        swaggerDoc.Tags = new List<OpenApiTag>
        {
            //new OpenApiTag { Name = "你的控制器名字，不带Controller", Description = "控制器描述" },
            // new OpenApiTag { Name = "FormCollectionObject", Description = "这是描述" },
        };
    }
}