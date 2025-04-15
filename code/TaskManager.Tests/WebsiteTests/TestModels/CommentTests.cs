using System;
using System.Collections.Generic;
using Xunit;
using TaskManagerData.Models;
using Task = TaskManagerData.Models.Task;

namespace TaskManager.Tests.WebsiteTests.TestModels
{
    public class CommentTests
    {
        [Fact]
        public void Comment_Id_CanBeSetAndRetrieved()
        {
            var comment = new Comment { Id = 42 };
            Assert.Equal(42, comment.Id);
        }

        [Fact]
        public void Comment_UserId_CanBeSetAndRetrieved()
        {
            var comment = new Comment { UserId = 10 };
            Assert.Equal(10, comment.UserId);
        }

        [Fact]
        public void Comment_User_CanBeSetAndRetrieved()
        {
            var user = new User { Id = 1, UserName = "TestUser" };
            var comment = new Comment { User = user };
            Assert.NotNull(comment.User);
            Assert.Equal("TestUser", comment.User.UserName);
        }

        [Fact]
        public void Comment_TaskId_CanBeSetAndRetrieved()
        {
            var comment = new Comment { TaskId = 5 };
            Assert.Equal(5, comment.TaskId);
        }

        [Fact]
        public void Comment_Task_CanBeSetAndRetrieved()
        {
            // Assume Task has at least an Id property, adjust accordingly based on your Task class
            var task = new Task { Id = 100 };
            var comment = new Comment { Task = task };
            Assert.NotNull(comment.Task);
            Assert.Equal(100, comment.Task.Id);
        }

        [Fact]
        public void Comment_Content_CanBeSetAndRetrieved()
        {
            var comment = new Comment { Content = "This is a sample comment" };
            Assert.Equal("This is a sample comment", comment.Content);
        }

        [Fact]
        public void Comment_Timestamp_CanBeSetAndRetrieved()
        {
            var now = DateTime.Now;
            var comment = new Comment { Timestamp = now };
            Assert.Equal(now, comment.Timestamp);
        }

        [Fact]
        public void Comment_ParentCommentId_CanBeSetAndRetrieved()
        {
            var comment = new Comment { ParentCommentId = 25 };
            Assert.Equal(25, comment.ParentCommentId);
        }

        [Fact]
        public void Comment_ParentComment_CanBeSetAndRetrieved()
        {
            var parentComment = new Comment { Id = 1, Content = "Parent comment" };
            var comment = new Comment { ParentComment = parentComment };
            Assert.NotNull(comment.ParentComment);
            Assert.Equal(1, comment.ParentComment.Id);
            Assert.Equal("Parent comment", comment.ParentComment.Content);
        }

        [Fact]
        public void Comment_Replies_DefaultsToEmptyList()
        {
            var comment = new Comment();
            Assert.NotNull(comment.Replies);
            Assert.Empty(comment.Replies);
        }

        [Fact]
        public void Comment_Replies_CanBeAssignedAndRetrieved()
        {
            // Arrange: Create a parent comment and two replies
            var parentComment = new Comment { Id = 1, Content = "Parent comment" };

            var reply1 = new Comment { Id = 2, Content = "Reply 1", ParentComment = parentComment, ParentCommentId = parentComment.Id };
            var reply2 = new Comment { Id = 3, Content = "Reply 2", ParentComment = parentComment, ParentCommentId = parentComment.Id };

            // Act: Add replies to the parent's Replies collection
            parentComment.Replies.Add(reply1);
            parentComment.Replies.Add(reply2);

            // Assert
            Assert.Equal(2, parentComment.Replies.Count);
            Assert.Contains(parentComment.Replies, r => r.Id == 2 && r.Content == "Reply 1");
            Assert.Contains(parentComment.Replies, r => r.Id == 3 && r.Content == "Reply 2");
        }

        [Fact]
        public void Comment_NestedReplies_WorkCorrectly()
        {
            // Arrange: Create a hierarchy of comments: parent -> reply -> nested reply
            var parentComment = new Comment { Id = 1, Content = "Parent comment" };
            var reply = new Comment
            {
                Id = 2,
                Content = "First reply",
                ParentComment = parentComment,
                ParentCommentId = parentComment.Id
            };
            var nestedReply = new Comment
            {
                Id = 3,
                Content = "Nested reply",
                ParentComment = reply,
                ParentCommentId = reply.Id
            };

            // Act: Build the nested reply hierarchy
            parentComment.Replies.Add(reply);
            reply.Replies.Add(nestedReply);

            // Assert hierarchy
            Assert.Single(parentComment.Replies);
            Assert.Equal(1, parentComment.Replies.Count);
            Assert.Equal("First reply", parentComment.Replies.First().Content);

            Assert.Single(reply.Replies);
            Assert.Equal("Nested reply", reply.Replies.First().Content);
        }
    }
}
