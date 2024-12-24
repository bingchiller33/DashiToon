import React, { Suspense } from "react";
import { NextPageProps } from "@/utils/nextjs";
import SearchScreen from "@/app/(SC_03)/_screens/SearchScreen";

export default function SearchPage(props: NextPageProps) {
    return (
        <Suspense>
            <SearchScreen />
        </Suspense>
    );
}
