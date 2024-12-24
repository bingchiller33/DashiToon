import React from "react";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogTrigger } from "@/components/ui/dialog";
import { MessageCircle } from "lucide-react";
import ChapterComment from "@/app/(Reader)/components/ChapterComment";
import { FaRegComment } from "react-icons/fa";

interface CommentDialogProps {
    chapterId: string;
    currentTheme: {
        bgColor: string;
        textColor: string;
    };
    buttonVariant?: "icon" | "default";
    className?: string;
}

const CommentDialog = ({ chapterId, currentTheme, buttonVariant = "default", className = "" }: CommentDialogProps) => {
    return (
        <Dialog>
            <DialogTrigger asChild>
                {buttonVariant === "icon" ? (
                    <Button
                        variant="ghost"
                        size="icon"
                        className={`${currentTheme.textColor} ${className} ${currentTheme.bgColor}`}
                    >
                        <FaRegComment className="h-6 w-6" />
                    </Button>
                ) : (
                    <Button
                        variant="outline"
                        className={`flex items-center gap-2 ${className} ${currentTheme.textColor} ${currentTheme.bgColor}`}
                    >
                        <MessageCircle className="h-4 w-4" />
                        Show Comments
                    </Button>
                )}
            </DialogTrigger>
            <DialogContent
                className={`max-w-full sm:max-w-[500px] md:max-w-[600px] lg:max-w-[800px] ${currentTheme.bgColor} max-h-[90vh] overflow-y-auto`}
            >
                <ChapterComment
                    bgColor={currentTheme.bgColor}
                    textColor={currentTheme.textColor}
                    chapterId={chapterId}
                />
            </DialogContent>
        </Dialog>
    );
};

export default CommentDialog;
