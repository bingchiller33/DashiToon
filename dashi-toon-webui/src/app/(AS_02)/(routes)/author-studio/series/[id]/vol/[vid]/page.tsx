import React from "react";
import ChapterListScreen from "@/app/(AS_02)/_screens/ChapterListScreen";
import { NextPageProps } from "@/utils/nextjs";

export default function AU_01_Page(props: NextPageProps) {
    const { searchParams, params } = props;
    const { id, vid } = params;

    const pageParams = searchParams["page"];
    const page = pageParams ? parseInt(pageParams) : 1;

    const pageStatus = searchParams["status"];
    const status = pageStatus ? parseInt(pageStatus) : 0;

    return (
        <ChapterListScreen
            seriesId={id}
            volumeId={vid}
            page={page}
            search={searchParams["search"] ?? ""}
            status={status}
        />
    );
}
