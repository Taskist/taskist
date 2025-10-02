using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Taskist.Web.Helpers.ModelBinding;

public class MetadataProvider : IDisplayMetadataProvider
{
    public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
    {
        var additionalValues = context.Attributes.OfType<IModelAttribute>().ToList();

        foreach (var additionalValue in additionalValues)
        {
            if (context.DisplayMetadata.AdditionalValues.ContainsKey(additionalValue.Name))
                throw new Exception($"There is already an attribute with the name '{additionalValue.Name}' on this model");

            context.DisplayMetadata.AdditionalValues.Add(additionalValue.Name, additionalValue);
        }
    }
}