using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using ApiAuthVerifyToken.V1.Domain;
using ApiAuthVerifyToken.V1.Factories;
using ApiAuthVerifyToken.V1.Infrastructure;
using System;
using System.Linq;

namespace ApiAuthVerifyToken.V1.Gateways
{
    public class DynamoDBGateway : IDynamoDbGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        public DynamoDBGateway(IDynamoDBContext dynamoDbContext)
        {
            _dynamoDbContext = dynamoDbContext;
        }

        public APIDataUserFlow GetAPIDataByApiGatewayIdAsync(string apiGatewayId)
        {
            try
            {
                var table = _dynamoDbContext.GetTargetTable<APIDataUserFlowDbEntity>();
                var search = table.Query(
                    new QueryOperationConfig
                    {
                        IndexName = "apiGatewayIdIndex",
                        Filter = new QueryFilter("apiGatewayId", QueryOperator.Equal, apiGatewayId)
                    });

                var documents = search.GetRemainingAsync().Result;
                if (documents.Count == 0)
                    throw new APIEntryNotFoundException($"API with id {apiGatewayId} does not exist in DynamoDB");
                if (documents.Count > 1)
                    LambdaLogger.Log($"WARNING: Multiple entries found for API with API Gateway ID {apiGatewayId} in DynamoDB: {documents.Select(d => d.ToJson())} - only the first will be returned");

                var selectedDocument = documents.First();
                var entity = _dynamoDbContext.FromDocument<APIDataUserFlowDbEntity>(selectedDocument);
                return entity?.ToDomain();
            }
            catch (Exception ex)
            {
                LambdaLogger.Log($"An error occurred retrieving data from DynamoDb while querying for {apiGatewayId}. Message: {ex.Message}");
                throw;
            }
        }

        public APIDataUserFlow GetAPIDataByNameAndEnvironmentAsync(string apiName, string environment)
        {
            try
            {
                var queryResult = _dynamoDbContext.QueryAsync<APIDataUserFlowDbEntity>(apiName, QueryOperator.Equal, new object[] { environment });

                var results = queryResult.GetRemainingAsync().Result;

                if (results.Count == 0)
                {
                    LambdaLogger.Log($"API with name {apiName} for environment {environment} does not exist in DynamoDB");
                    throw new APIEntryNotFoundException();
                }

                return results[0]?.ToDomain();
            }
            catch (Exception ex)
            {
                LambdaLogger.Log($"An error occurred retrieving data from DynamoDb while querying for {apiName} in {environment} environment. Message: {ex.Message}");
                throw;
            }
        }
    }
}
