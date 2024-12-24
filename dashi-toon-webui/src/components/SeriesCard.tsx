/* eslint-disable @next/next/no-img-element */
import { Badge } from "@/components/ui/badge";
import useUwU from "@/hooks/useUwU";
import { SeriesResp2 } from "@/utils/api/series";
import { STATUS_CONFIG_2 } from "@/utils/consts";
import cx from "classnames";
import { ThumbsUp, UserPen } from "lucide-react";
import Link from "next/link";

export const placeholder = "/images/atg.webp";

export default function SeriesCard(props: { data: SeriesResp2 }) {
    const [uwu] = useUwU();
    const img = uwu ? placeholder : props.data.thumbnail;

    return (
        <Link href={`/series/${props.data.id}`} className="group overflow-hidden text-sm">
            <div className="relative">
                <img className="aspect-[3/4] w-full" src={img} alt="Ảnh thu nhỏ" />
                <p className="absolute right-0 top-0 bg-black/50 p-1">
                    {props?.data?.type === 2 ? "Truyện tranh" : "Tiểu thuyết"}
                </p>
                <div className="absolute bottom-0 max-h-full w-full translate-y-full bg-black/50 from-transparent to-black/60 pb-2 transition-all group-hover:translate-y-0">
                    <div className="-translate-y-full bg-gradient-to-b from-transparent via-black/40 via-20% to-black/60 p-2 pb-1 pt-8 transition-all group-hover:translate-y-0 group-hover:via-transparent group-hover:to-transparent">
                        <p className="text-sm font-bold">{props?.data?.title}</p>
                        <div className="flex items-center gap-1 text-sm">
                            <ThumbsUp size={14} /> {props.data.rating?.toFixed(1) ?? 90} %
                        </div>
                        <p className="mb-1 flex items-center gap-2">
                            <span className="flex items-center gap-2">
                                <span
                                    className={cx(
                                        "h-2 w-2 rounded-full",
                                        STATUS_CONFIG_2[props?.data?.status ?? 1]?.color,
                                    )}
                                />
                                <span className="text-sm font-medium">
                                    {STATUS_CONFIG_2[props?.data?.status ?? 1]?.label}
                                </span>
                            </span>
                        </p>
                    </div>
                    <div className="px-2">
                        <p className="">
                            <UserPen size={14} className="inline" /> {props.data.authors?.slice(0, 2).join(", ")}
                        </p>
                        <p className="font-bold">Thể loại:</p>
                        <div className="flex items-center gap-1">
                            {props.data.genres?.slice(0, 2)?.map((x) => <Badge key={x}>{x}</Badge>)}
                        </div>

                        {props.data.synopsis && <p className="mt-2 font-bold">Tóm tắt:</p>}
                        <p>{props.data.synopsis}</p>
                    </div>
                </div>
            </div>
        </Link>
    );
}
