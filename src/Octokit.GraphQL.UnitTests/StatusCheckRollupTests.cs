using System.Linq;
using Octokit.GraphQL.Core.Builders;
using Octokit.GraphQL.Model;
using Xunit;

namespace Octokit.GraphQL.UnitTests
{
    public class StatusCheckRollupTests
    {
        [Fact]
        public void Commit_StatusCheckRollup_Contexts_Allows_Null_CheckRun_DetailsUrl()
        {
            var query = new Query()
                .Repository("SimCorp", "IMS")
                .Object("15a18500a72775cd0eed9aae76799ffa043781e2")
                .Cast<Commit>()
                .Select(commit => new
                {
                    StatusCheckRollup = commit.StatusCheckRollup != null ? new
                    {
                        commit.StatusCheckRollup.State,
                        Contexts = commit.StatusCheckRollup.Contexts(100, null, null, null).Nodes
                            .Select(ctx => ctx.Switch<object>(when => when
                                .CheckRun(cr => new
                                {
                                    Name = cr.Name,
                                    State = (object)(cr.Conclusion ?? CheckConclusionState.Neutral),
                                    TargetUrl = cr.DetailsUrl
                                })
                                .StatusContext(sc => new
                                {
                                    Name = sc.Context,
                                    State = (object)sc.State,
                                    TargetUrl = sc.TargetUrl
                                })))
                            .ToList()
                    } : null
                })
                .Compile();

            const string data = @"{
  ""data"": {
    ""repository"": {
      ""object"": {
        ""statusCheckRollup"": {
          ""state"": ""PENDING"",
          ""contexts"": {
            ""nodes"": [
              {
                ""__typename"": ""CheckRun"",
                ""name"": ""Build"",
                ""conclusion"": null,
                ""detailsUrl"": null
              },
              {
                ""__typename"": ""StatusContext"",
                ""context"": ""legacy-status"",
                ""state"": ""SUCCESS"",
                ""targetUrl"": null
              }
            ]
          }
        }
      }
    }
  }
}";

            var result = query.Deserialize(data);
            var contexts = result.StatusCheckRollup.Contexts.Cast<dynamic>().ToList();

            Assert.Equal(2, contexts.Count);
            Assert.Equal("Build", (string)contexts[0].Name);
            Assert.Null((string)contexts[0].TargetUrl);
            Assert.Equal("legacy-status", (string)contexts[1].Name);
            Assert.Null((string)contexts[1].TargetUrl);
        }

        [Fact]
        public void Commit_StatusCheckRollup_Contexts_Allows_Null_Node()
        {
            var query = new Query()
                .Repository("SimCorp", "IMS")
                .Object("15a18500a72775cd0eed9aae76799ffa043781e2")
                .Cast<Commit>()
                .Select(commit => new
                {
                    StatusCheckRollup = commit.StatusCheckRollup != null ? new
                    {
                        commit.StatusCheckRollup.State,
                        Contexts = commit.StatusCheckRollup.Contexts(100, null, null, null).Nodes
                            .Select(ctx => ctx.Switch<object>(when => when
                                .CheckRun(cr => new
                                {
                                    Name = cr.Name,
                                    State = (object)(cr.Conclusion ?? CheckConclusionState.Neutral),
                                    TargetUrl = cr.DetailsUrl
                                })
                                .StatusContext(sc => new
                                {
                                    Name = sc.Context,
                                    State = (object)sc.State,
                                    TargetUrl = sc.TargetUrl
                                })))
                            .ToList()
                    } : null
                })
                .Compile();

            const string data = @"{
  ""data"": {
    ""repository"": {
      ""object"": {
        ""statusCheckRollup"": {
          ""state"": ""PENDING"",
          ""contexts"": {
            ""nodes"": [
              null,
              {
                ""__typename"": ""CheckRun"",
                ""name"": ""Build"",
                ""conclusion"": null,
                ""detailsUrl"": null
              }
            ]
          }
        }
      }
    }
  }
}";

            var result = query.Deserialize(data);
            var contexts = result.StatusCheckRollup.Contexts.Cast<dynamic>().ToList();

            Assert.Equal(2, contexts.Count);
            Assert.Null(contexts[0]);
            Assert.Equal("Build", (string)contexts[1].Name);
        }
    }
}
