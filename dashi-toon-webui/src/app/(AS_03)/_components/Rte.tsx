"use client";

import React, { useEffect, useRef } from "react";
import Highlight from "@tiptap/extension-highlight";
import TextAlign from "@tiptap/extension-text-align";
import { EditorContent, useEditor, Editor } from "@tiptap/react";
import StarterKit from "@tiptap/starter-kit";

import {
    BsTypeH1,
    BsTypeH2,
    BsTypeH3,
    BsTypeBold,
    BsTypeItalic,
    BsTypeUnderline,
    BsTypeStrikethrough,
    BsLink,
} from "react-icons/bs";
import { RiText } from "react-icons/ri";
import { RxDividerHorizontal } from "react-icons/rx";
import { Separator } from "@/components/ui/separator";
import { FaUndo, FaRedo } from "react-icons/fa";
import { Button } from "@/components/ui/button";
import { BsFileEarmarkImage } from "react-icons/bs";
import Underline from "@tiptap/extension-underline";
import { Toggle } from "@/components/ui/toggle";
import { Link } from "@tiptap/extension-link";
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
} from "@/components/ui/popover";
import Dropcursor from "@tiptap/extension-dropcursor";
import Image from "@tiptap/extension-image";
import { openFilePicker } from "@/utils";
import { getImage } from "@/utils/api/image";
import { uploadChapterImage } from "@/utils/api/author-studio/chapter";
import { toast } from "sonner";

const CustomImage = Image.extend({
    addAttributes() {
        return {
            ...this.parent?.(),
            imgName: {
                default: null,
                renderHTML(attributes) {
                    return {
                        "data-img-name": attributes.imgName,
                    };
                },
                parseHTML(element) {
                    return element.getAttribute("data-img-name");
                },
            },
        };
    },
});

async function handleUploadImages(editor: Editor, files: File[]) {
    for (const item of files) {
        const [up, err] = await uploadChapterImage(item);
        if (err) {
            toast.error("Tải hình ảnh lên không thành công", {});
            return;
        }

        const [imgUrl, err2] = await getImage(up.fileName, "chapters");
        if (err2) {
            toast.error("Tải hình ảnh lên không thành công", {});
            return;
        }

        editor
            ?.chain()
            .focus()
            .insertContentAt(editor?.state.selection.to, {
                type: "image",
                attrs: {
                    src: imgUrl.imageUrl,
                    imgName: up.fileName,
                },
            })
            .run();
    }

    toast.success("Hình ảnh đã được tải lên thành công", {});
}

const MenuBar = ({ editor }: { editor: Editor }) => {
    const linkRef = useRef<HTMLInputElement>(null);
    async function handleImage() {
        const files = await openFilePicker();
        if (!files) {
            return;
        }

        if (!editor) {
            return;
        }

        await handleUploadImages(editor, files);
    }

    if (!editor) {
        return null;
    }

    return (
        <div className="control-group">
            <div className="button-group flex flex-wrap items-center">
                <Button type="button" variant={"ghost"} className="px-3">
                    <FaUndo size={12} />
                </Button>

                <Button type="button" variant={"ghost"} className="px-3">
                    <FaRedo size={12} />
                </Button>

                <Separator
                    orientation="vertical"
                    className="h-6 bg-neutral-600"
                />

                <Toggle
                    pressed={editor.isActive("heading", { level: 1 })}
                    disabled={!editor.can().toggleHeading({ level: 1 })}
                    onClick={() =>
                        editor.chain().focus().toggleHeading({ level: 1 }).run()
                    }
                >
                    <BsTypeH1 size={12} />
                </Toggle>
                <Toggle
                    pressed={editor.isActive("heading", { level: 2 })}
                    disabled={!editor.can().toggleHeading({ level: 2 })}
                    onClick={() =>
                        editor.chain().focus().toggleHeading({ level: 2 }).run()
                    }
                >
                    <BsTypeH2 size={12} />
                </Toggle>
                <Toggle
                    pressed={editor.isActive("heading", { level: 3 })}
                    disabled={!editor.can().toggleHeading({ level: 3 })}
                    onClick={() =>
                        editor.chain().focus().toggleHeading({ level: 3 }).run()
                    }
                >
                    <BsTypeH3 size={12} />
                </Toggle>
                <Toggle
                    value="text"
                    pressed={editor.isActive("paragraph")}
                    disabled={!editor.can().setParagraph()}
                    onClick={() => editor.chain().focus().setParagraph().run()}
                >
                    <RiText size={12} />
                </Toggle>
                <Separator
                    orientation="vertical"
                    className="h-6 bg-neutral-600"
                />
                <Toggle
                    pressed={editor.isActive("bold")}
                    onClick={() => editor.chain().focus().toggleBold().run()}
                    disabled={!editor.can().toggleBold()}
                >
                    <BsTypeBold size={12} />
                </Toggle>
                <Toggle
                    pressed={editor.isActive("italic")}
                    onClick={() => editor.chain().focus().toggleItalic().run()}
                    disabled={!editor.can().toggleItalic()}
                >
                    <BsTypeItalic size={12} />
                </Toggle>
                <Toggle
                    pressed={editor.isActive("underline")}
                    onClick={() =>
                        editor.chain().focus().toggleUnderline().run()
                    }
                    disabled={!editor.can().toggleUnderline()}
                >
                    <BsTypeUnderline size={12} />
                </Toggle>
                <Toggle
                    pressed={editor.isActive("strike")}
                    onClick={() => editor.chain().focus().toggleStrike().run()}
                    disabled={!editor.can().toggleStrike()}
                >
                    <BsTypeStrikethrough size={12} />
                </Toggle>
                <Separator
                    orientation="vertical"
                    className="h-6 bg-neutral-600"
                />
                <Popover>
                    <PopoverTrigger>
                        <Toggle
                            value="link"
                            pressed={editor.isActive("link")}
                            disabled={!editor.can().toggleLink({ href: "" })}
                        >
                            <BsLink size={12} />
                        </Toggle>
                    </PopoverTrigger>
                    <PopoverContent>
                        <div className="rounded-lg shadow-lg">
                            <div className="flex items-center justify-between gap-2">
                                <input
                                    ref={linkRef}
                                    type="text"
                                    placeholder="URL"
                                    className="w-full rounded-lg border border-neutral-600 p-2"
                                />
                                <Button
                                    type="button"
                                    variant="default"
                                    onClick={() =>
                                        editor
                                            .chain()
                                            .focus()
                                            .toggleLink({
                                                href:
                                                    linkRef.current?.value ??
                                                    "",
                                            })
                                            .run()
                                    }
                                >
                                    Link
                                </Button>
                            </div>
                        </div>
                    </PopoverContent>
                </Popover>

                <Separator
                    orientation="vertical"
                    className="h-6 bg-neutral-600"
                />

                <Button
                    type="button"
                    variant={"ghost"}
                    className="px-3"
                    onClick={() =>
                        editor.chain().focus().setHorizontalRule().run()
                    }
                >
                    <RxDividerHorizontal size={12} />
                </Button>

                <Button
                    type="button"
                    variant={"ghost"}
                    className="px-3"
                    onClick={handleImage}
                >
                    <BsFileEarmarkImage size={12} />
                </Button>
            </div>
        </div>
    );
};

export interface RteProps {
    content: string;
    onChange: (content: string) => void;
}

export default function Rte(props: RteProps) {
    const editor = useEditor({
        editorProps: {
            attributes: {
                class: "overflow-auto flex-grow h-[50vh] overflow-y-auto prose lg:prose-xl prose-neutral prose-invert max-w-full",
            },
            handleDrop: function (view, event, slice, moved) {
                if (
                    !moved &&
                    event.dataTransfer &&
                    event.dataTransfer.files &&
                    event.dataTransfer.files[0]
                ) {
                    handleUploadImages(
                        editor!,
                        Array.from(event.dataTransfer.files),
                    );
                    return true; // handled
                }

                return false; // not handled use default behaviour
            },
        },
        extensions: [
            StarterKit,
            TextAlign.configure({
                types: ["heading", "paragraph"],
            }),
            Underline.configure({}),
            Link.configure({}),
            Highlight,
            CustomImage,
            Dropcursor,
        ],
        onUpdate: ({ editor }) => {
            props.onChange(editor.getHTML());
        },
        immediatelyRender: false,
    });

    useEffect(() => {
        if (editor && props.content !== editor.getHTML()) {
            editor.commands.setContent(props.content);
        }
    }, [props.content, editor]);

    return (
        <div className="flex flex-grow flex-col rounded-lg border border-neutral-600">
            <MenuBar editor={editor!} />
            <Separator className="bg-neutral-600" />
            <EditorContent
                editor={editor}
                className="flex flex-grow px-2 py-4"
            />
        </div>
    );
}
