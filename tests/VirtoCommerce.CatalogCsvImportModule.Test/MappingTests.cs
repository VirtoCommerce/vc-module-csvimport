using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using AutoFixture;
using CsvHelper;
using CsvHelper.Configuration;
using FluentAssertions;
using VirtoCommerce.CatalogCsvImportModule.Core.Model;
using VirtoCommerce.CatalogCsvImportModule.Data.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using Xunit;

namespace VirtoCommerce.CatalogCsvImportModule.Test
{
    public class MappingTests
    {
        [Fact]
        public void CsvProductMapTest_CsvHasPropertyValues_PropertyValuesMapped()
        {
            var importInfo = new CsvImportInfo { Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration() };
            importInfo.Configuration.CsvColumns = new[] { "Sku" };
            importInfo.Configuration.PropertyCsvColumns = new[] { "ProductProperty", "ProductProperty_Multivalue" };
            importInfo.Configuration.Delimiter = ",";

            string path = GetDataFilePath("product-propertyvalues.csv");
            var csvProducts = ReadCsvFile(path, importInfo);

            Action<PropertyValue>[] inspectorsFirstProduct = {
                x => Assert.True((string) x.Value == "Product-1-propertyvalue-test" && x.PropertyName =="ProductProperty"),
                x => Assert.True((string) x.Value == "Product-1-multivalue-1, Product-1-multivalue-2" && x.PropertyName =="ProductProperty_Multivalue")
            };
            Action<PropertyValue>[] inspectorsSecond = {
                x => Assert.True((string) x.Value == "Product-2-propertyvalue-test" && x.PropertyName =="ProductProperty"),
                x => Assert.True((string) x.Value == "Product-2-multivalue-1, Product-2-multivalue-1, Product-2-multivalue-3" && x.PropertyName =="ProductProperty_Multivalue")
            };
            Assert.Collection(csvProducts.FirstOrDefault().Properties.SelectMany(x => x.Values), inspectorsFirstProduct);
            Assert.Collection(csvProducts.LastOrDefault().Properties.SelectMany(x => x.Values), inspectorsSecond);
        }

        [Fact]
        public void CsvProductMapTest_CsvHasProductProperties_PropertiesMapped()
        {
            var importInfo = new CsvImportInfo { Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration() };
            importInfo.Configuration.Delimiter = ",";

            string path = GetDataFilePath("product-productproperties.csv");
            var csvProducts = ReadCsvFile(path, importInfo);
            var product = csvProducts.FirstOrDefault();

            Assert.Equal("429408", product.Id);
            Assert.Equal("CBLK21113", product.Sku);
            Assert.Equal("cblk21113-product-1", product.Name);
            Assert.Equal("catId_1", product.CategoryId);
            Assert.Equal("GTIN_Value", product.Gtin);
            Assert.Equal("mainprod_123", product.MainProductId);
            Assert.Equal("Vendor_value", product.Vendor);
            Assert.Equal("ProductType_value", product.ProductType);
            Assert.Equal("ShippingType_value", product.ShippingType);
            Assert.Equal("DownloadType_value", product.DownloadType);
            Assert.True(product.HasUserAgreement);
            Assert.True(product.IsBuyable);
            Assert.True(product.TrackInventory);
        }

        [Fact]
        public void CsvProductMapTest_CsvHasPriceAndQuantity_PriceAndQuantityMapped()
        {
            var importInfo = new CsvImportInfo { Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration() };
            importInfo.Configuration.Delimiter = ",";

            string path = GetDataFilePath("product-productproperties-priceQuantity.csv");
            var csvProducts = ReadCsvFile(path, importInfo);
            var product = csvProducts.FirstOrDefault();

            Assert.Equal("123.4", product.ListPrice);
            Assert.Equal(123.4m, product.Price.List);
            Assert.Equal("456.7", product.SalePrice);
            Assert.Equal(456.7m, product.Price.Sale);
            Assert.Equal("EUR", product.Currency);
            Assert.Equal("EUR", product.Price.Currency);
            Assert.Equal("5", product.PriceMinQuantity);
            Assert.Equal(5, product.Price.MinQuantity);
        }

        [Fact]
        public void CsvProductMapTest_CsvHasSeoInfo_SeoInfoMapped()
        {
            var importInfo = new CsvImportInfo { Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration() };
            importInfo.Configuration.Delimiter = ",";

            string path = GetDataFilePath("product-productproperties-seoInfo.csv");
            var csvProducts = ReadCsvFile(path, importInfo);
            var product = csvProducts.FirstOrDefault(x => x.Code == "cblk21113-product-1");

            Assert.Equal("seo-slug-url", product.SeoUrl);
            Assert.Equal("seo-slug-url", product.SeoInfo.SemanticUrl);
            Assert.Equal("Seo_Title_Value", product.SeoTitle);
            Assert.Equal("Seo_Title_Value", product.SeoInfo.PageTitle);
            Assert.Equal("Seo_Descr_Value", product.SeoDescription);
            Assert.Equal("Seo_Descr_Value", product.SeoInfo.MetaDescription);
            Assert.Equal("Seo_Language_Value", product.SeoInfo.LanguageCode);
            Assert.True(csvProducts.Count == 2);
        }

        [Fact]
        public void CsvProductMapTest_CsvHasReview_ReviewMapped()
        {
            var importInfo = new CsvImportInfo { Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration() };
            importInfo.Configuration.Delimiter = ",";

            string path = GetDataFilePath("product-productproperties-review.csv");
            var csvProducts = ReadCsvFile(path, importInfo);
            var product = csvProducts.FirstOrDefault();

            Assert.Equal("Review_Content", product.Review);
            Assert.Equal("Review_Content", product.EditorialReview.Content);
            Assert.Equal("ReviewType_Value", product.ReviewType);
            Assert.Equal("ReviewType_Value", product.EditorialReview.ReviewType);
        }

        [Fact]
        public void CsvProductMapTest_CsvHasCategorypath_CategoryPathMapped()
        {
            var importInfo = new CsvImportInfo { Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration() };
            importInfo.Configuration.Delimiter = ",";

            string path = GetDataFilePath("product-productproperties-categoryPath.csv");
            var csvProducts = ReadCsvFile(path, importInfo);
            var product = csvProducts.FirstOrDefault();

            Assert.Equal("TestCategory1", product.CategoryPath);
            Assert.Equal("TestCategory1", product.Category.Path);
        }

        [Fact]
        public void CsvProductMapTest_MappingHasDefaultCategoryPath_DefaultCategoryPathMapped()
        {
            var defaultValue = "Custom_category_path_value";

            var importInfo = new CsvImportInfo { Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration() };
            importInfo.Configuration.Delimiter = ",";

            var categoryPathMapping = importInfo.Configuration.PropertyMaps.FirstOrDefault(x => x.EntityColumnName == "CategoryPath");
            categoryPathMapping.CsvColumnName = null;
            categoryPathMapping.CustomValue = defaultValue;

            string path = GetDataFilePath("product-productproperties-noCategoryPath.csv");
            var csvProducts = ReadCsvFile(path, importInfo);
            var product = csvProducts.FirstOrDefault();

            Assert.Equal(defaultValue, product.CategoryPath);
            Assert.Equal(defaultValue, product.Category.Path);
        }

        [Fact]
        public void CsvProductMapTest_MappingHasDefaultBoolValue_DefaultBoolValuesMapped()
        {
            var defaultValue = true;

            var importInfo = new CsvImportInfo { Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration() };
            importInfo.Configuration.Delimiter = ",";

            var categoryPathMapping = importInfo.Configuration.PropertyMaps.FirstOrDefault(x => x.EntityColumnName == "IsBuyable");
            categoryPathMapping.CsvColumnName = null;
            categoryPathMapping.CustomValue = defaultValue.ToString();

            string path = GetDataFilePath("product-productproperties.csv");
            var csvProducts = ReadCsvFile(path, importInfo);
            var product = csvProducts.FirstOrDefault();

            Assert.True(product.IsBuyable);
        }

        [Fact]
        public void CsvProductMapTest_CsvHasBooleanValues_BooleanFieldsMapped()
        {
            var importInfo = new CsvImportInfo { Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration() };
            importInfo.Configuration.Delimiter = ",";

            string path = GetDataFilePath("product-productproperties-boolean.csv");
            var csvProducts = ReadCsvFile(path, importInfo);

            Assert.False(csvProducts[0].HasUserAgreement);
            Assert.False(csvProducts[0].IsBuyable);
            Assert.False(csvProducts[0].TrackInventory);

            Assert.True(csvProducts[1].HasUserAgreement);
            Assert.True(csvProducts[1].IsBuyable);
            Assert.True(csvProducts[1].TrackInventory);
        }

        [Fact]
        public void CsvProductMapTest_CsvHasMultipleLines_LineNumberMapTest()
        {
            var importInfo = new CsvImportInfo { Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration() };
            importInfo.Configuration.Delimiter = ",";

            string path = GetDataFilePath("product-productproperties-twoproducts.csv");
            var csvProducts = ReadCsvFile(path, importInfo);

            Assert.Equal(2, csvProducts[0].LineNumber);
            Assert.Equal(3, csvProducts[1].LineNumber);
        }

        //Export mapping test

        [Fact]
        public void CsvHeadersExportTest_DefaultConfiguration_HeadersAreSame()
        {
            using (var sw = new StringWriter())
            {
                using (var csv = new CsvWriter(sw))
                {
                    var exportInfo = new CsvExportInfo();
                    exportInfo.Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration();
                    csv.Configuration.Delimiter = exportInfo.Configuration.Delimiter;
                    csv.Configuration.RegisterClassMap(new CsvProductMap(exportInfo.Configuration));

                    csv.WriteHeader<CsvProduct>();
                    csv.Flush();

                    var expected = string.Join(exportInfo.Configuration.Delimiter, exportInfo.Configuration.PropertyMaps.Select(x => x.CsvColumnName));

                    Assert.Equal(expected, sw.ToString());
                }
            }
        }

        [Fact]
        public void CsvProductMapTest_DictionaryMultilanguage_OnlyOneAliasExported()
        {
            //Arrange
            var product = GetProduct();
            product.Properties = new List<Property>
            {
                new Property()
                {
                    Id = "property1",
                    Name = "Dictionary_Multilanguage",
                    Dictionary = true,
                    Multilanguage = true,
                    Values = new List<PropertyValue>
                    {
                        new PropertyValue { Alias = "A", Value = "EN_A", ValueType = PropertyValueType.ShortText },
                        new PropertyValue { Alias = "A", Value = "DE_A", ValueType = PropertyValueType.ShortText }
                    }
                }
            };

            //Act
            var importedCsvProduct = ExportAndImportProduct(product);

            //Assert
            importedCsvProduct.Properties.Should().HaveCount(1);
            importedCsvProduct.Properties.First().Values.Should().HaveCount(1);
            importedCsvProduct.Properties.First().Values.First().Value.ToString().Should().BeEquivalentTo("A");
        }

        [Fact]
        public void CsvProductMapTest_DictionaryMultivalue_OnlyUniqAliasesExported()
        {
            //Arrange
            var product = GetProduct();
            product.Properties = new List<Property>
            {
                new Property()
                {
                    Id = "property1",
                    Name = "Dictionary_Multivalue",
                    Dictionary = true,
                    Multilanguage = false,
                    Multivalue = true,
                    Values = new List<PropertyValue>
                    {
                        new PropertyValue { Alias = "A", Value = "EN_A", ValueType = PropertyValueType.ShortText },
                        new PropertyValue { Alias = "A", Value = "DE_A", ValueType = PropertyValueType.ShortText },
                        new PropertyValue { Alias = "B", Value = "EN_B", ValueType = PropertyValueType.ShortText },
                        new PropertyValue { Alias = "B", Value = "DE_B", ValueType = PropertyValueType.ShortText }
                    }
                }
            };

            //Act
            var importedCsvProduct = ExportAndImportProduct(product);

            //Assert
            importedCsvProduct.Properties.Should().HaveCount(1);
            importedCsvProduct.Properties.First().Values.Should().HaveCount(1);
            importedCsvProduct.Properties.First().Values.First().Value.ToString().Should().BeEquivalentTo("A;B");
        }


        [Fact]
        public void CsvProductMapTest_Multilanguage_AllValueExported()
        {
            //Arrange
            var product = GetProduct();
            product.Properties = new List<Property>
            {
                new Property()
                {
                    Id = "property1",
                    Name = "Multilanguage",
                    Dictionary = false,
                    Multilanguage = true,
                    Values = new List<PropertyValue>
                    {
                        new PropertyValue { Value = "EN_A", ValueType = PropertyValueType.ShortText },
                        new PropertyValue { Value = "DE_A", ValueType = PropertyValueType.ShortText }
                    }
                }
            };

            //Act
            var importedCsvProduct = ExportAndImportProduct(product);

            //Assert
            importedCsvProduct.Properties.Should().HaveCount(1);
            importedCsvProduct.Properties.First().Values.Should().HaveCount(1);
            importedCsvProduct.Properties.First().Values.First().Value.ToString().Should().BeEquivalentTo("EN_A;DE_A");
        }


        // Support methods
        private List<CsvProduct> ReadCsvFile(string path, CsvImportInfo importInfo)
        {
            var csvProducts = new List<CsvProduct>();
#pragma warning disable S3966 // Objects should not be disposed more than once - no problem in this case
            using (var fs = File.Open(path, FileMode.Open))
#pragma warning restore S3966 // Objects should not be disposed more than once
            {
                using (var reader = new CsvReader(new StreamReader(fs)))
                {
                    reader.Configuration.Delimiter = importInfo.Configuration.Delimiter;
                    reader.Configuration.RegisterClassMap(new CsvProductMap(importInfo.Configuration));
                    reader.Configuration.MissingFieldFound = (strings, i, arg3) =>
                    {
                        //do nothing
                    };
                    reader.Configuration.TrimOptions = TrimOptions.Trim;
                    reader.Configuration.HeaderValidated = null;

                    while (reader.Read())
                    {
                        var csvProduct = reader.GetRecord<CsvProduct>();
                        csvProducts.Add(csvProduct);
                    }
                }
            }
            return csvProducts;
        }

        private string GetDataFilePath(string fileName)
        {
            return $"../../../data/{fileName}";
        }

        private CatalogProduct GetProduct()
        {
            var fixture = new Fixture();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture.Build<CatalogProduct>()
                .With(x => x.Variations, new List<Variation>())
                .With(x => x.Associations, new List<ProductAssociation>())
                .With(x => x.ReferencedAssociations, new List<ProductAssociation>())
                .With(x => x.PropertyValues, new List<PropertyValue>())
                .Create();
        }

        private CsvProduct ExportAndImportProduct(CatalogProduct product)
        {
            var exportInfo = new CsvExportInfo();
            using (var stream = new MemoryStream())
            {
                var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true) { AutoFlush = true };
                using (var csvWriter = new CsvWriter(streamWriter))
                {
                    exportInfo.Configuration = CsvProductMappingConfiguration.GetDefaultConfiguration();
                    exportInfo.Configuration.PropertyCsvColumns = product.Properties.Select(x => x.Name).Distinct().ToArray();
                    csvWriter.Configuration.Delimiter = exportInfo.Configuration.Delimiter;
                    csvWriter.Configuration.RegisterClassMap(new CsvProductMap(exportInfo.Configuration));

                    csvWriter.WriteHeader<CsvProduct>();
                    csvWriter.NextRecord();
                    var csvProduct = new CsvProduct(product, null, null, null, null);
                    csvWriter.WriteRecord(csvProduct);
                    csvWriter.Flush();
                    stream.Position = 0;
                }

                using (var reader = new CsvReader(new StreamReader(stream, Encoding.UTF8)))
                {
                    reader.Configuration.Delimiter = exportInfo.Configuration.Delimiter;
                    reader.Configuration.RegisterClassMap(new CsvProductMap(exportInfo.Configuration));
                    reader.Configuration.MissingFieldFound = (strings, i, arg3) =>
                    {
                        //do nothing
                    };
                    reader.Configuration.TrimOptions = TrimOptions.Trim;
                    reader.Read();
                    return reader.GetRecord<CsvProduct>();
                }
            }

        }
    }
}
