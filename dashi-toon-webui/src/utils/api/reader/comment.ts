import { Result } from "../../err";
import { fetchApi } from "..";

export interface Comment {
    id: string;
    content: string;
    likes: number;
    dislikes: number;
    repliesCount: number;
    userId: string;
    username: string;
    userAvatar: string;
    commentDate: string;
    isEdited: boolean;
}

export interface CommentResponse {
    items: Comment[];
    pageNumber: number;
    totalPages: number;
    totalCount: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

interface GetCommentsParams {
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
}

export async function getChapterComments(
    chapterId: string,
    params: GetCommentsParams = {},
): Promise<Result<CommentResponse>> {
    return await fetchApi("GET", `/api/Chapters/${chapterId}/comments`, undefined, {
        params: {
            pageNumber: params.pageNumber ?? 1,
            pageSize: params.pageSize,
            sortBy: params.sortBy,
        },
    });
}

export async function createComment(chapterId: string, content: string): Promise<Result<Comment>> {
    return await fetchApi("POST", `/api/Chapters/${chapterId}/comments`, {
        chapterId,
        content,
    });
}

export interface CommentWithReplies extends Comment {
    replies: string[];
}

export async function getCommentReplies(chapterId: string, commentId: string): Promise<Result<CommentWithReplies[]>> {
    return await fetchApi("GET", `/api/Chapters/${chapterId}/comments/${commentId}/replies`);
}

export async function createCommentReply(
    chapterId: string,
    commentId: string,
    content: string,
): Promise<Result<Comment>> {
    return await fetchApi("POST", `/api/Chapters/${chapterId}/comments/${commentId}/replies`, {
        chapterId,
        commentId,
        content,
    });
}

export async function updateComment(chapterId: string, commentId: string, content: string): Promise<Result<Comment>> {
    return await fetchApi("PUT", `/api/Chapters/${chapterId}/comments/${commentId}`, {
        chapterId,
        commentId,
        content,
    });
}

export async function rateComment(chapterId: string, commentId: string, isLiked: boolean): Promise<Result<void>> {
    return await fetchApi("POST", `/api/Chapters/${chapterId}/comments/${commentId}/rate`, {
        chapterId,
        commentId,
        isLiked,
    });
}
