"use client";

import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { MessageCircle, ThumbsUp, ThumbsDown, SendHorizontal, AlertCircle } from "lucide-react";
import {
    getChapterComments,
    createComment,
    type Comment,
    getCommentReplies,
    createCommentReply,
    updateComment,
    rateComment,
    CommentResponse,
} from "@/utils/api/reader/comment";
import { formatDateAgoWithRange } from "@/lib/date-ago";
import { toast } from "sonner";
import { getUserInfo } from "@/utils/api/user";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import {
    Pagination,
    PaginationContent,
    PaginationItem,
    PaginationLink,
    PaginationNext,
    PaginationPrevious,
} from "@/components/ui/pagination";
import { reportComment } from "@/utils/api/reader/report";
import { ReportDialog } from "@/components/ReportDialog";
import { ScrollArea } from "@/components/ui/scroll-area";
import { isAllowedToReviewOrComment, Permission } from "@/utils/api/moderator/moderation";

interface ComponentProps {
    bgColor: string;
    textColor: string;
    chapterId: string;
}

interface User {
    userId: string;
    username: string;
    email: string;
}

export default function Component({ bgColor, textColor, chapterId }: ComponentProps) {
    const [comments, setComments] = useState<Comment[]>([]);
    const [newComment, setNewComment] = useState("");
    const [isLoading, setIsLoading] = useState(false);
    const [repliesMap, setRepliesMap] = useState<Record<string, Comment[]>>({});
    const [replyContent, setReplyContent] = useState<Record<string, string>>({});
    const [showReplies, setShowReplies] = useState<Record<string, boolean>>({});
    const [loadingReplies, setLoadingReplies] = useState<Record<string, boolean>>({});
    const [replyingTo, setReplyingTo] = useState<Record<string, string>>({});
    const [editingComment, setEditingComment] = useState<string | null>(null);
    const [editContent, setEditContent] = useState<string>("");
    const [user, setUser] = useState<User | null>(null);
    const [sortBy, setSortBy] = useState<string>("newest");
    const [totalComments, setTotalComments] = useState<number>(0);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [data, setData] = useState<CommentResponse | null>(null);
    const [commentPermission, setCommentPermission] = useState<Permission | null>(null);
    const PAGE_SIZE = 10;

    useEffect(() => {
        const fetchComments = async () => {
            const [data, error] = await getChapterComments(chapterId, {
                sortBy,
                pageNumber: currentPage,
                pageSize: PAGE_SIZE,
            });
            if (error) {
                console.error("Error fetching comments:", error);
                toast.error("Không thể tải bình luận");
            } else if (data) {
                setComments(data.items);
                setData(data);
                setTotalComments(data.totalCount);
                setTotalPages(data.totalPages);
            }
        };
        fetchComments();
    }, [chapterId, sortBy, currentPage]);

    useEffect(() => {
        const fetchUser = async () => {
            const [data, error] = await getUserInfo();
            if (error) {
                console.error("Error fetching user info:", error);
            } else if (data) {
                setUser(data);
            }
        };
        fetchUser();
    }, []);

    useEffect(() => {
        const checkCommentPermission = async () => {
            const [data, error] = await isAllowedToReviewOrComment();
            if (!error && data) {
                setCommentPermission(data);
            }
        };
        checkCommentPermission();
    }, []);
    console.log(commentPermission);

    const handleSubmitComment = async () => {
        if (!newComment.trim()) return;
        if (!commentPermission?.isAllowed) {
            toast.error("Bạn không có quyền bình luận");
            return;
        }
        setIsLoading(true);

        const [data, error] = await createComment(chapterId, newComment);
        if (error) {
            console.error("Error posting comment:", error);
            toast.error("Failed to post comment");
        } else if (data) {
            setComments([...comments, data]);
            setNewComment("");
            toast.success("Comment posted successfully");
        }

        setIsLoading(false);
    };

    const handleLoadReplies = async (commentId: string) => {
        if (showReplies[commentId]) {
            setShowReplies((prev) => ({ ...prev, [commentId]: false }));
            return;
        }

        setLoadingReplies((prev) => ({ ...prev, [commentId]: true }));

        const [data, error] = await getCommentReplies(chapterId, commentId);
        if (error) {
            console.error("Error fetching replies:", error);
            toast.error("Failed to load replies");
        } else if (data) {
            setRepliesMap((prev) => ({ ...prev, [commentId]: data }));
            setShowReplies((prev) => ({ ...prev, [commentId]: true }));
        }

        setLoadingReplies((prev) => ({ ...prev, [commentId]: false }));
    };

    const handleSubmitReply = async (commentId: string) => {
        const content = replyContent[commentId]?.trim();
        if (!content) return;

        setIsLoading(true);

        const parentCommentId =
            comments.find(
                (comment) =>
                    comment.id === commentId || repliesMap[comment.id]?.some((reply) => reply.id === commentId),
            )?.id || commentId;

        const [data, error] = await createCommentReply(chapterId, parentCommentId, content);

        if (error) {
            console.error("Error posting reply:", error);
            toast.error("Failed to post reply");
        } else if (data) {
            const [repliesData] = await getCommentReplies(chapterId, parentCommentId);
            if (repliesData) {
                setRepliesMap((prev) => ({
                    ...prev,
                    [parentCommentId]: repliesData,
                }));
            }
            setReplyContent((prev) => ({ ...prev, [commentId]: "" }));
            setReplyingTo((prev) => {
                const newState = { ...prev };
                delete newState[commentId];
                return newState;
            });
            toast.success("Reply posted successfully");
        }

        setIsLoading(false);
    };

    const handleReplyClick = (commentId: string, username: string) => {
        setShowReplies((prev) => ({ ...prev, [commentId]: true }));
        setReplyingTo((prev) => {
            const newState = { ...prev };
            Object.keys(newState).forEach((key) => delete newState[key]);
            newState[commentId] = username;
            return newState;
        });
    };

    const handleSubmitEdit = async (commentId: string) => {
        if (!editContent.trim()) return;
        setIsLoading(true);

        const [data, error] = await updateComment(chapterId, commentId, editContent);
        if (error) {
            console.error("Error updating comment:", error);
            toast.error("Failed to update comment");
        } else if (data) {
            setComments((prev) => prev.map((comment) => (comment.id === commentId ? { ...data } : comment)));
            setRepliesMap((prev) => {
                const newRepliesMap = { ...prev };
                Object.keys(newRepliesMap).forEach((parentId) => {
                    newRepliesMap[parentId] = newRepliesMap[parentId].map((reply) =>
                        reply.id === commentId ? { ...data } : reply,
                    );
                });
                return newRepliesMap;
            });
            setEditingComment(null);
            toast.success("Comment updated successfully");
        }

        setIsLoading(false);
    };

    const handleRate = async (commentId: string, isLike: boolean) => {
        try {
            const [_, error] = await rateComment(chapterId, commentId, isLike);
            if (error) {
                console.error("Error rating comment:", error);
                toast.error("Failed to rate comment");
                return;
            }

            const [commentsData] = await getChapterComments(chapterId, {
                sortBy,
            });
            if (commentsData) {
                setComments(commentsData.items);
            }

            const parentComment = comments.find((comment) =>
                repliesMap[comment.id]?.some((reply) => reply.id === commentId),
            );
            if (parentComment) {
                const [repliesData] = await getCommentReplies(chapterId, parentComment.id);
                if (repliesData) {
                    setRepliesMap((prev) => ({
                        ...prev,
                        [parentComment.id]: repliesData,
                    }));
                }
            }
        } catch (error) {
            console.error("Error:", error);
            toast.error("Something went wrong");
        }
    };

    const handlePageChange = (newPage: number) => {
        setCurrentPage(newPage);
        // Scroll to the top of the comments section
        document.querySelector(".comment-section")?.scrollIntoView({ behavior: "smooth" });
    };

    const handleSortChange = (value: string) => {
        setSortBy(value);
        setCurrentPage(1);
    };

    const handleReport = async (commentId: string, reason: string) => {
        try {
            const [error] = await reportComment(commentId, reason);
            if (error) {
                console.error("Error reporting comment:", error);
                return;
            }
        } catch (error) {
            console.error("Error:", error);
        }
    };

    return (
        <div className={`w-full ${bgColor}`}>
            <div className={`mx-auto max-w-4xl space-y-4 p-4`}>
                <div className="mb-4 flex items-center justify-between">
                    <div className={`flex items-center gap-2 ${textColor}`}>
                        <MessageCircle className="h-5 w-5" />
                        <h2 className="text-base font-semibold">Bình Luận ({totalComments})</h2>
                    </div>

                    <Select value={sortBy} onValueChange={(value) => handleSortChange(value)}>
                        <SelectTrigger className="w-[180px]">
                            <SelectValue placeholder="Sắp xếp theo" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="newest">Mới nhất</SelectItem>
                            <SelectItem value="oldest">Cũ nhất</SelectItem>
                            <SelectItem value="top">Nổi bật</SelectItem>
                        </SelectContent>
                    </Select>
                </div>

                {user ? (
                    commentPermission?.isAllowed ? (
                        <div
                            className="space-y-4 rounded-lg border p-4"
                            style={{
                                backgroundColor: `color-mix(in srgb, ${bgColor} 85%, black)`,
                            }}
                        >
                            <div className="flex gap-2">
                                <Textarea
                                    placeholder="Viết bình luận..."
                                    value={newComment}
                                    onChange={(e) => setNewComment(e.target.value)}
                                    className={`min-h-[80px] ${bgColor} ${textColor} border focus:ring-0`}
                                />
                            </div>
                            <div className="flex justify-end pt-2">
                                <Button size="sm" onClick={handleSubmitComment} disabled={isLoading}>
                                    <SendHorizontal className="mr-2 h-4 w-4" />
                                    Bình Luận
                                </Button>
                            </div>
                        </div>
                    ) : (
                        <div className="rounded-lg border border-red-200 bg-red-50 p-4">
                            <div className="flex items-center gap-3 text-red-800">
                                <AlertCircle className="h-5 w-5" />
                                <div className="space-y-1">
                                    <h3 className="font-medium">Tài khoản của bạn đã bị hạn chế bình luận</h3>
                                    {commentPermission?.notAllowedUntil && (
                                        <p className="text-sm">
                                            Bạn có thể bình luận lại sau:{" "}
                                            {new Date(commentPermission.notAllowedUntil).toLocaleDateString("vi-VN", {
                                                year: "numeric",
                                                month: "long",
                                                day: "numeric",
                                                hour: "2-digit",
                                                minute: "2-digit",
                                            })}
                                        </p>
                                    )}
                                </div>
                            </div>
                        </div>
                    )
                ) : (
                    <div className="rounded-lg border border-yellow-200 bg-yellow-50 p-4">
                        <div className="flex items-center gap-3 text-yellow-800">
                            <AlertCircle className="h-5 w-5" />
                            <p>Vui lòng đăng nhập để bình luận</p>
                        </div>
                    </div>
                )}

                <div className="pb-10">
                    <ScrollArea className="h-[600px] rounded-md">
                        <div className="space-y-4 py-4">
                            {comments.map((comment) => (
                                <div key={comment.id} className="comment-thread">
                                    {/* Main Comment */}
                                    <div className={`flex gap-3 ${textColor}`}>
                                        <Avatar className="h-8 w-8">
                                            <AvatarImage src={comment.userAvatar} alt={comment.username} />
                                            <AvatarFallback>{comment.username[0]}</AvatarFallback>
                                        </Avatar>

                                        <div className="flex-1 space-y-1">
                                            <div className="flex items-center gap-2 text-base">
                                                <span className="font-bold">{comment.username}</span>
                                                <span className="text-sm text-muted-foreground">
                                                    {formatDateAgoWithRange(comment.commentDate)}{" "}
                                                    {comment.isEdited && (
                                                        <span className="text-xs text-muted-foreground">(Đã sửa)</span>
                                                    )}
                                                </span>
                                            </div>

                                            {editingComment === comment.id ? (
                                                <div className="space-y-2">
                                                    <Textarea
                                                        value={editContent}
                                                        onChange={(e) => setEditContent(e.target.value)}
                                                        className="min-h-[40px] resize-none"
                                                    />
                                                    <div className="flex justify-end gap-2">
                                                        <Button
                                                            variant="ghost"
                                                            size="sm"
                                                            onClick={() => {
                                                                setEditingComment(null);
                                                                setEditContent("");
                                                            }}
                                                        >
                                                            Hủy
                                                        </Button>
                                                        <Button
                                                            size="sm"
                                                            onClick={() => handleSubmitEdit(comment.id)}
                                                            disabled={isLoading}
                                                        >
                                                            Lưu
                                                        </Button>
                                                    </div>
                                                </div>
                                            ) : (
                                                <>
                                                    <p className="text-base">{comment.content}</p>
                                                </>
                                            )}

                                            {/* Comment Actions */}
                                            <div className="flex items-center gap-4 pt-1">
                                                <div className="flex items-center gap-2">
                                                    <button
                                                        className="flex items-center gap-1 text-base text-muted-foreground transition-colors hover:text-blue-500"
                                                        onClick={() => handleRate(comment.id, true)}
                                                    >
                                                        <ThumbsUp className="h-4 w-4" />
                                                        {comment.likes}
                                                    </button>
                                                    <button
                                                        className="flex items-center gap-1 text-base text-muted-foreground transition-colors hover:text-blue-500"
                                                        onClick={() => handleRate(comment.id, false)}
                                                    >
                                                        <ThumbsDown className="h-4 w-4" />
                                                        {comment.dislikes}
                                                    </button>
                                                </div>
                                                <button
                                                    className="text-base text-muted-foreground transition-colors hover:text-blue-500"
                                                    onClick={() => handleReplyClick(comment.id, comment.username)}
                                                >
                                                    Trả lời
                                                </button>
                                                {user?.userId === comment.userId && (
                                                    <button
                                                        className="text-base text-muted-foreground transition-colors hover:text-blue-500"
                                                        onClick={() => {
                                                            setEditingComment(comment.id);
                                                            setEditContent(comment.content);
                                                        }}
                                                    >
                                                        Sửa
                                                    </button>
                                                )}
                                                {user?.userId !== comment.userId && (
                                                    <ReportDialog
                                                        contentId={comment.id}
                                                        contentType="comment"
                                                        username={comment.username}
                                                        onReport={handleReport}
                                                    />
                                                )}
                                            </div>

                                            {/* Reply Input (shown when replying) */}
                                            {replyingTo[comment.id] && (
                                                <div className="mt-3 flex gap-3 pl-8 pt-8">
                                                    <Avatar className="h-8 w-8">
                                                        <AvatarImage src="/default-avatar.png" alt="Your avatar" />
                                                        <AvatarFallback>{comment.username[0]}</AvatarFallback>
                                                    </Avatar>
                                                    <div className="flex-1">
                                                        <Textarea
                                                            placeholder={`Trả lời @${replyingTo[comment.id]}...`}
                                                            value={replyContent[comment.id] || ""}
                                                            onChange={(e) =>
                                                                setReplyContent((prev) => ({
                                                                    ...prev,
                                                                    [comment.id]: e.target.value,
                                                                }))
                                                            }
                                                            className="min-h-[40px] resize-none"
                                                        />
                                                        <div className="mt-2 flex justify-end gap-2">
                                                            <Button
                                                                variant="ghost"
                                                                size="sm"
                                                                onClick={() => {
                                                                    setReplyingTo((prev) => {
                                                                        const newState = {
                                                                            ...prev,
                                                                        };
                                                                        delete newState[comment.id];
                                                                        return newState;
                                                                    });
                                                                    setReplyContent((prev) => ({
                                                                        ...prev,
                                                                        [comment.id]: "",
                                                                    }));
                                                                }}
                                                            >
                                                                Hủy
                                                            </Button>
                                                            <Button
                                                                size="sm"
                                                                onClick={() => handleSubmitReply(comment.id)}
                                                                disabled={isLoading}
                                                            >
                                                                Trả lời
                                                            </Button>
                                                        </div>
                                                    </div>
                                                </div>
                                            )}

                                            {/* Replies Section */}
                                            {comment.repliesCount > 0 && (
                                                <button
                                                    className="mt-4 text-base font-medium text-blue-500 transition-colors hover:text-blue-600"
                                                    onClick={() => handleLoadReplies(comment.id)}
                                                >
                                                    {loadingReplies[comment.id]
                                                        ? "Đang tải..."
                                                        : showReplies[comment.id]
                                                          ? "Ẩn trả lời"
                                                          : `Xem trả lời (${comment.repliesCount})`}
                                                </button>
                                            )}

                                            {/* Replies List */}
                                            {repliesMap[comment.id]?.length > 0 && (
                                                <div
                                                    className={`grid transition-all duration-300 ease-in-out ${
                                                        showReplies[comment.id] ? "grid-rows-[1fr]" : "grid-rows-[0fr]"
                                                    } `}
                                                >
                                                    <div className="overflow-hidden">
                                                        <div className="mt-3 space-y-3 pl-8">
                                                            {repliesMap[comment.id].map((reply) => (
                                                                <div key={reply.id} className="flex gap-3">
                                                                    <Avatar className="h-8 w-8">
                                                                        <AvatarImage
                                                                            src={reply.userAvatar}
                                                                            alt={reply.username}
                                                                        />
                                                                        <AvatarFallback>
                                                                            {reply.username[0]}
                                                                        </AvatarFallback>
                                                                    </Avatar>
                                                                    <div className="flex-1 space-y-1">
                                                                        <div className="flex items-center gap-2 text-base">
                                                                            <span className="font-bold">
                                                                                {reply.username}
                                                                            </span>
                                                                            <span className="text-muted-foreground">
                                                                                {formatDateAgoWithRange(
                                                                                    reply.commentDate,
                                                                                )}{" "}
                                                                                {reply.isEdited && (
                                                                                    <span className="text-xs text-muted-foreground">
                                                                                        (Đã sửa)
                                                                                    </span>
                                                                                )}
                                                                            </span>
                                                                        </div>
                                                                        {editingComment === reply.id ? (
                                                                            <div className="space-y-2">
                                                                                <Textarea
                                                                                    value={editContent}
                                                                                    onChange={(e) =>
                                                                                        setEditContent(e.target.value)
                                                                                    }
                                                                                    className="min-h-[40px] resize-none"
                                                                                />
                                                                                <div className="flex justify-end gap-2">
                                                                                    <Button
                                                                                        variant="ghost"
                                                                                        size="sm"
                                                                                        onClick={() => {
                                                                                            setEditingComment(null);
                                                                                            setEditContent("");
                                                                                        }}
                                                                                    >
                                                                                        Hủy
                                                                                    </Button>
                                                                                    <Button
                                                                                        size="sm"
                                                                                        onClick={() =>
                                                                                            handleSubmitEdit(reply.id)
                                                                                        }
                                                                                        disabled={isLoading}
                                                                                    >
                                                                                        Lưu
                                                                                    </Button>
                                                                                </div>
                                                                            </div>
                                                                        ) : (
                                                                            <>
                                                                                <p className="text-base">
                                                                                    {reply.content
                                                                                        .split(
                                                                                            /(@\w+(?:(?!\.com)[^\s]*)?)/,
                                                                                        )
                                                                                        .map((part, index) => {
                                                                                            if (part.startsWith("@")) {
                                                                                                return (
                                                                                                    <strong key={index}>
                                                                                                        {part}
                                                                                                    </strong>
                                                                                                );
                                                                                            }
                                                                                            return part;
                                                                                        })}
                                                                                </p>
                                                                            </>
                                                                        )}
                                                                        <div className="flex items-center gap-4 pt-1">
                                                                            <button
                                                                                className="flex items-center gap-1 text-base text-muted-foreground transition-colors hover:text-blue-500"
                                                                                onClick={() =>
                                                                                    handleRate(reply.id, true)
                                                                                }
                                                                            >
                                                                                <ThumbsUp className="h-4 w-4" />
                                                                                {reply.likes}
                                                                            </button>
                                                                            <button
                                                                                className="flex items-center gap-1 text-base text-muted-foreground transition-colors hover:text-blue-500"
                                                                                onClick={() =>
                                                                                    handleRate(reply.id, false)
                                                                                }
                                                                            >
                                                                                <ThumbsDown className="h-4 w-4" />
                                                                                {reply.dislikes}
                                                                            </button>
                                                                            <button
                                                                                className="text-base text-muted-foreground transition-colors hover:text-blue-500"
                                                                                onClick={() =>
                                                                                    handleReplyClick(
                                                                                        reply.id,
                                                                                        reply.username,
                                                                                    )
                                                                                }
                                                                            >
                                                                                Trả lời
                                                                            </button>
                                                                            {user?.userId === reply.userId && (
                                                                                <button
                                                                                    className="text-base text-muted-foreground transition-colors hover:text-blue-500"
                                                                                    onClick={() => {
                                                                                        setEditingComment(reply.id);
                                                                                        setEditContent(reply.content);
                                                                                    }}
                                                                                >
                                                                                    Sửa
                                                                                </button>
                                                                            )}
                                                                            <ReportDialog
                                                                                contentId={comment.id}
                                                                                contentType="comment"
                                                                                username={comment.username}
                                                                                onReport={handleReport}
                                                                            />
                                                                        </div>
                                                                        {replyingTo[reply.id] && (
                                                                            <div className="mt-3 flex gap-3">
                                                                                <Avatar className="h-8 w-8">
                                                                                    <AvatarImage
                                                                                        src="/default-avatar.png"
                                                                                        alt="Your avatar"
                                                                                    />
                                                                                    <AvatarFallback>
                                                                                        {reply.username[0]}
                                                                                    </AvatarFallback>
                                                                                </Avatar>
                                                                                <div className="flex-1">
                                                                                    <Textarea
                                                                                        placeholder={`Trả lời @${replyingTo[reply.id]}...`}
                                                                                        value={
                                                                                            replyContent[reply.id] || ""
                                                                                        }
                                                                                        onChange={(e) =>
                                                                                            setReplyContent((prev) => ({
                                                                                                ...prev,
                                                                                                [reply.id]:
                                                                                                    e.target.value,
                                                                                            }))
                                                                                        }
                                                                                        className="min-h-[40px] resize-none"
                                                                                    />
                                                                                    <div className="mt-2 flex justify-end gap-2">
                                                                                        <Button
                                                                                            variant="ghost"
                                                                                            size="sm"
                                                                                            onClick={() => {
                                                                                                setReplyingTo(
                                                                                                    (prev) => {
                                                                                                        const newState =
                                                                                                            {
                                                                                                                ...prev,
                                                                                                            };
                                                                                                        delete newState[
                                                                                                            reply.id
                                                                                                        ];
                                                                                                        return newState;
                                                                                                    },
                                                                                                );
                                                                                                setReplyContent(
                                                                                                    (prev) => ({
                                                                                                        ...prev,
                                                                                                        [reply.id]: "",
                                                                                                    }),
                                                                                                );
                                                                                            }}
                                                                                        >
                                                                                            Hủy
                                                                                        </Button>
                                                                                        <Button
                                                                                            size="sm"
                                                                                            onClick={() =>
                                                                                                handleSubmitReply(
                                                                                                    reply.id,
                                                                                                )
                                                                                            }
                                                                                            disabled={isLoading}
                                                                                        >
                                                                                            Trả lời
                                                                                        </Button>
                                                                                    </div>
                                                                                </div>
                                                                            </div>
                                                                        )}
                                                                    </div>
                                                                </div>
                                                            ))}
                                                        </div>
                                                    </div>
                                                </div>
                                            )}
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </ScrollArea>

                    {comments && totalPages > 1 && (
                        <Pagination className="mt-4">
                            <PaginationContent>
                                {/* Previous button */}
                                {data?.hasPreviousPage ? (
                                    <PaginationItem>
                                        <PaginationPrevious onClick={() => handlePageChange(currentPage - 1)} />
                                    </PaginationItem>
                                ) : (
                                    <PaginationItem>
                                        <PaginationPrevious className="pointer-events-none opacity-50" />
                                    </PaginationItem>
                                )}

                                {/* Page numbers */}
                                {Array.from({ length: totalPages }, (_, i) => i + 1).map((page) => (
                                    <PaginationItem key={page}>
                                        <PaginationLink
                                            onClick={() => handlePageChange(page)}
                                            isActive={page === currentPage}
                                        >
                                            {page}
                                        </PaginationLink>
                                    </PaginationItem>
                                ))}

                                {/* Next button */}
                                {data?.hasNextPage ? (
                                    <PaginationItem>
                                        <PaginationNext onClick={() => handlePageChange(currentPage + 1)} />
                                    </PaginationItem>
                                ) : (
                                    <PaginationItem>
                                        <PaginationNext className="pointer-events-none opacity-50" />
                                    </PaginationItem>
                                )}
                            </PaginationContent>
                        </Pagination>
                    )}
                </div>
            </div>
        </div>
    );
}
