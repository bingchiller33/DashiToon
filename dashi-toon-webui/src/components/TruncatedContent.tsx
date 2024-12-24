import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";

interface TruncatedContentProps {
    content: string;
    title: string;
    maxLength?: number;
    maxLines?: number;
}

export function TruncatedContent({ content, title, maxLength = 100, maxLines = 2 }: TruncatedContentProps) {
    return (
        <div className="max-w-xs">
            <Dialog>
                <DialogTrigger asChild>
                    <Button variant="link" className="h-auto p-0 text-left hover:no-underline">
                        <span
                            className={`line-clamp-${maxLines} text-sm`}
                            style={{
                                display: "-webkit-box",
                                WebkitLineClamp: maxLines,
                                WebkitBoxOrient: "vertical",
                                overflow: "hidden",
                                wordBreak: "break-word",
                            }}
                        >
                            {content}
                        </span>
                    </Button>
                </DialogTrigger>
                <DialogContent className="max-w-2xl">
                    <DialogHeader>
                        <DialogTitle>{title}</DialogTitle>
                    </DialogHeader>
                    <div className="mt-4 max-h-[60vh] overflow-y-auto whitespace-pre-wrap">{content}</div>
                </DialogContent>
            </Dialog>
        </div>
    );
}
