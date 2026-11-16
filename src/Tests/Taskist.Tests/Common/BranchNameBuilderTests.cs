using Taskist.Core.Common;
using Taskist.Core.Enums;

namespace Taskist.Tests.Common;

public class BranchNameBuilderTests
{
    #region Prefix

    [Theory]
    [InlineData(new[] { (int)TaskTypeGroup.Bug }, "bugfix")]
    [InlineData(new[] { (int)TaskTypeGroup.Bug, (int)TaskTypeGroup.Bug }, "bugfix")]
    [InlineData(new[] { (int)TaskTypeGroup.ToDo }, "task")]
    [InlineData(new[] { (int)TaskTypeGroup.CR }, "feature")]
    [InlineData(new[] { (int)TaskTypeGroup.Bug, (int)TaskTypeGroup.CR }, "feature")]
    public void PrefixFor_PicksThePrefixFromTheTaskTypes(int[] groups, string expected)
    {
        Assert.Equal(expected, BranchNameBuilder.PrefixFor(groups));
    }

    #endregion

    #region Build

    [Fact]
    public void Build_CombinesPrefixIdAndSlug()
    {
        Assert.Equal("feature/12-fix-login-crash", BranchNameBuilder.Build("feature", [12], "Fix login: crash!", null));
    }

    [Fact]
    public void Build_WithKeyPrefix_SortsAndDeduplicatesIds()
    {
        Assert.Equal("bugfix/TSK-3-TSK-5-title", BranchNameBuilder.Build("bugfix", [5, 3, 5], "Title", " tsk "));
    }

    [Fact]
    public void Build_WithManyTasks_SummarisesTheIds()
    {
        Assert.Equal("feature/1-plus-3-batch", BranchNameBuilder.Build("feature", [4, 1, 2, 3], "Batch", null));
    }

    [Fact]
    public void Build_WithoutTitle_OmitsTheSlug()
    {
        Assert.Equal("task/7", BranchNameBuilder.Build("task", [7], "  ", null));
    }

    [Fact]
    public void Build_NeverExceedsTheMaximumLength()
    {
        var name = BranchNameBuilder.Build(new string('p', 90), [123456], "a very long title that keeps going", null);

        Assert.True(name.Length <= 100);
        Assert.False(name.EndsWith('-'));
    }

    #endregion

    #region Slugify

    [Theory]
    [InlineData("Café déjà vu", "cafe-deja-vu")]
    [InlineData("  --Hello,   World!!--  ", "hello-world")]
    [InlineData("日本語 title", "title")]
    [InlineData(null, "")]
    public void Slugify_ProducesLowercaseAsciiSlugs(string? text, string expected)
    {
        Assert.Equal(expected, BranchNameBuilder.Slugify(text, 40));
    }

    [Fact]
    public void Slugify_TruncatesWithoutATrailingDash()
    {
        Assert.Equal("abc", BranchNameBuilder.Slugify("abc def", 4));
    }

    #endregion

    #region Extract ids

    [Theory]
    [InlineData("feature/12-34-fix-thing", null, new[] { 12, 34 })]
    [InlineData("feature/TSK-12-TSK-34-fix", "tsk", new[] { 12, 34 })]
    [InlineData("12-quick", null, new[] { 12 })]
    [InlineData("feature/fix-12", null, new int[0])]
    [InlineData("main", null, new int[0])]
    [InlineData(null, null, new int[0])]
    public void ExtractTaskIds_ReadsLeadingIdsFromTheBranchName(string? branch, string? key, int[] expected)
    {
        Assert.Equal(expected, BranchNameBuilder.ExtractTaskIds(branch, key));
    }

    [Fact]
    public void ExtractTaskReferences_FindsHashMentionsKeysAndStandaloneNumbers()
    {
        var ids = BranchNameBuilder.ExtractTaskReferences("TSK", "Fixes #12 and tsk-77", "also 3456");

        Assert.Equal(new SortedSet<int> { 12, 77, 3456 }, new SortedSet<int>(ids));
    }

    [Theory]
    [InlineData("release v1.2.345")]
    [InlineData("on 2026-10-20")]
    [InlineData("see !123")]
    [InlineData("https://git.example.com/group/repo/-/issues/4567")]
    [InlineData("only 12 here")]
    [InlineData("build 1030:45")]
    public void ExtractTaskReferences_IgnoresVersionsDatesMergeRequestsAndUrls(string text)
    {
        Assert.Empty(BranchNameBuilder.ExtractTaskReferences(null, text));
    }

    #endregion

    #region Merge request matching

    [Fact]
    public void TasksForBranchMergeRequest_WithNoLinkedTasks_ReturnsNothing()
    {
        Assert.Empty(BranchNameBuilder.TasksForBranchMergeRequest([], new HashSet<int> { 1 }, "feature/1", null, "#1", null, false, 3));
    }

    [Fact]
    public void TasksForBranchMergeRequest_GroupBranch_MatchesEveryLinkedTask()
    {
        var result = BranchNameBuilder.TasksForBranchMergeRequest([10, 11], new HashSet<int> { 10, 11, 99 }, "feature/x", null, "#99", null, true, 3);

        Assert.Equal([10, 11], result);
    }

    [Fact]
    public void TasksForBranchMergeRequest_MatchesOnlyTheTasksNamedInTheTitle()
    {
        var result = BranchNameBuilder.TasksForBranchMergeRequest([10, 11], new HashSet<int> { 10, 11 }, "feature/shared", null, "#10 fix", null, false, 3);

        Assert.Equal([10], result);
    }

    [Fact]
    public void TasksForBranchMergeRequest_CountsIdsInTheBranchName()
    {
        var result = BranchNameBuilder.TasksForBranchMergeRequest([10, 11], new HashSet<int> { 10, 11 }, "feature/11-thing", null, "No refs", null, false, 3);

        Assert.Equal([11], result);
    }

    [Fact]
    public void TasksForBranchMergeRequest_NamingOnlyAnotherKnownTask_MatchesNone()
    {
        Assert.Empty(BranchNameBuilder.TasksForBranchMergeRequest([10, 11], new HashSet<int> { 10, 11, 99 }, "feature/shared", null, "#99 fix", null, false, 3));
    }

    [Fact]
    public void TasksForBranchMergeRequest_NamingNoTask_FallsBackWithinTheLimit()
    {
        var result = BranchNameBuilder.TasksForBranchMergeRequest([10, 11], new HashSet<int> { 10, 11 }, "feature/shared", null, "Tidy up", null, false, 3);

        Assert.Equal([10, 11], result);
    }

    [Fact]
    public void TasksForBranchMergeRequest_NamingNoTask_AboveTheLimit_MatchesNone()
    {
        Assert.Empty(BranchNameBuilder.TasksForBranchMergeRequest([10, 11, 12, 13], new HashSet<int> { 10, 11, 12, 13 }, "feature/shared", null, "Tidy up", null, false, 3));
    }

    [Fact]
    public void TasksForBranchMergeRequest_ZeroLimit_StillAllowsASingleTask()
    {
        var result = BranchNameBuilder.TasksForBranchMergeRequest([10], new HashSet<int> { 10 }, "feature/shared", null, "Tidy up", null, false, 0);

        Assert.Equal([10], result);
    }

    #endregion

    #region Validate name

    [Theory]
    [InlineData("feature/12-fix")]
    [InlineData("hotfix/TSK-1")]
    public void ValidateName_AcceptsGitSafeNames(string name)
    {
        Assert.Null(BranchNameBuilder.ValidateName(name));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("has space")]
    [InlineData("a..b")]
    [InlineData("a~b")]
    [InlineData("a@{b")]
    [InlineData("a//b")]
    [InlineData("-leading")]
    [InlineData("trailing/")]
    [InlineData("name.lock")]
    [InlineData("ends.")]
    public void ValidateName_RejectsNamesGitWouldRefuse(string? name)
    {
        Assert.NotNull(BranchNameBuilder.ValidateName(name));
    }

    [Fact]
    public void ValidateName_RejectsOverlongNames()
    {
        Assert.NotNull(BranchNameBuilder.ValidateName(new string('a', 256)));
    }

    #endregion
}
