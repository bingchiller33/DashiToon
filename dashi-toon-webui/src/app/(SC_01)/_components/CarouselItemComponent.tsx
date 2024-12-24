import { Card, CardContent } from "@/components/ui/card";
import { CarouselItem } from "@/components/ui/carousel";
import { Badge } from "@/components/ui/badge";
import React from "react";
import Link from "next/link";

export default function CarouselItemComponent(props: { data: any }) {
    return (
        <CarouselItem className="basis-1/2 text-xs lg:basis-1/5">
            <Link href={`/series/${props.data.seriesId}`}>
                <Card>
                    <div className="relative">
                        <img
                            className="w-full"
                            style={{ aspectRatio: 266 / 350 }}
                            src={props.data.thumbnail}
                            alt=""
                        />
                        <div className="absolute left-0 top-0 bg-slate-700 p-1">
                            {props?.data?.status}
                        </div>
                    </div>
                    <CardContent className="px-1 py-2">
                        <div className="flex items-center justify-between">
                            <div className="flex items-center">
                                <Badge
                                    className="mr-1 px-0.5 uppercase"
                                    style={{
                                        fontSize: "10px",
                                        lineHeight: "12px",
                                    }}
                                >
                                    {props?.data?.type}
                                </Badge>
                                <p>Chap {props?.data?.currentChapter}</p>
                            </div>
                            <div>{props.data.timeAgo}</div>
                        </div>
                        <div className="text-sm">{props?.data?.title}</div>
                    </CardContent>
                </Card>
            </Link>
        </CarouselItem>
    );
}
