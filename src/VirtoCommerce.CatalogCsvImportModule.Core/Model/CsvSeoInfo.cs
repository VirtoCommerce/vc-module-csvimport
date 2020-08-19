using VirtoCommerce.CoreModule.Core.Seo;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogCsvImportModule.Core.Model
{
    public class CsvSeoInfo : SeoInfo
    {
        public virtual void MergeFrom(SeoInfo source)
        {
            SemanticUrl = source.SemanticUrl;
            LanguageCode = source.LanguageCode;
            StoreId = source.StoreId;
            ObjectId = source.ObjectId;
            ObjectType = source.ObjectType;
            Id = source.Id;
            
            if (PageTitle.IsNullOrEmpty())
            {
                PageTitle = source.PageTitle;
            }

            if (MetaDescription.IsNullOrEmpty())
            {
                MetaDescription = source.MetaDescription;
            }
        }
    }
}
