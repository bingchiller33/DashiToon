import React from "react";
import CarouselComponent from "./CarouselComponent";
import { RelatedSeries } from "@/utils/api/series";

export default function RelatedSeriesSection(props: { data: RelatedSeries[] }) {
    return (
        <div>
            <CarouselComponent
                heading="Có thể bạn cũng thích"
                data={props.data}
            />
        </div>
    );
}
