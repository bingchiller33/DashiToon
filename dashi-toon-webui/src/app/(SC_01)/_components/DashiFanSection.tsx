import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { DashiFanTier, getDashiFanTiers, getSeriesInfo, SeriesResp, subscribeDashiFanTier } from "@/utils/api/series";
import { getActiveSub, Subscription2 } from "@/utils/api/subscription";
import cx from "classnames";
import { CheckCircle, Zap } from "lucide-react";
import Link from "next/link";
import { useEffect, useState } from "react";
import { toast } from "sonner";
import DowngradeDashiFan from "./DowngradeDashiFan";
import UpgradeDashiFan from "./UpgradeDashiFan";
import { useInterval } from "usehooks-ts";

export interface DashiFanSectionProps {
    seriesId: string;
    pending?: boolean;
}

export default function DashiFanSection(props: DashiFanSectionProps) {
    const { seriesId, pending } = props;
    const [bestTier, setBestTier] = useState<number>(0);
    const [currentTier, setCurrentTier] = useState<Subscription2 | null>(null);
    const [tiers, setTiers] = useState<DashiFanTier[]>([]);
    const [seriesInfo, setSeriesInfo] = useState<SeriesResp>();
    const [checkTier, setCheckTier] = useState<boolean>(false);
    useInterval(
        async () => {
            const [tier, err] = await getActiveSub(seriesId, true);
            if (tier?.status === 2) {
                setCheckTier(false);
                setCurrentTier(tier);
                const newUrl = new URL(window.location.href);
                newUrl.searchParams.delete("successPayment");
                window.history.replaceState({}, "", newUrl.toString());
                toast.info("Đơn hàng của bạn đã được xử lý!", {
                    action: {
                        label: "Cập nhật",
                        onClick: () => (window.location.href = newUrl.toString()),
                    },
                });
            }
        },
        checkTier ? 5000 : null,
    );

    useEffect(() => {
        async function work() {
            const [seriesInfo, errInfo] = await getSeriesInfo(seriesId);
            if (errInfo) {
                toast.error("Failed to fetch series info");
                return;
            }

            setSeriesInfo(seriesInfo);

            const [data, err] = await getDashiFanTiers(seriesId);
            if (err) {
                toast.error("Không thể tải danh sách hạng DashiFan!");
                return;
            }

            setTiers(data);
            const bestTier = data.map((x) => x.perks).reduce((a, b) => Math.max(a, b), 0);
            setBestTier(bestTier);

            const [currTier, errCurr] = await getActiveSub(seriesId, true);
            if (pending) {
                setCheckTier(true);
            }
            if (currTier) {
                currTier?.status === 2 && setCurrentTier(currTier);
                setCurrentTier(currTier);

                console.log(currTier);
                if (currTier.status === 1) {
                    setCheckTier(true);
                }
            }
        }

        work();
    }, [seriesId, pending]);

    const handleSubscribe = async (tier: any) => {
        const url = new URL(window.location.href);
        url.searchParams.delete("successPayment");
        url.searchParams.delete("tierId");
        url.searchParams.delete("ba_token");
        url.searchParams.delete("subscription_id");
        url.searchParams.delete("token");

        url.searchParams.set("successPayment", "true");
        url.searchParams.set("tierId", tier.id);
        url.searchParams.set("tab", "dashifan");

        const [paymentUrl, err] = await subscribeDashiFanTier({
            seriesId: parseInt(seriesId),
            tierId: tier.id,
            returnUrl: url.toString(),
            cancelUrl: window.location.href,
        });

        if (err) {
            toast.error("Không thể đăng ký gói DashiFan!");
            return;
        }

        window.location.href = paymentUrl;
        setCurrentTier(tier);
    };

    const isChangeTier = currentTier?.status === 2;

    return (
        <div className="p-4 text-white sm:p-8">
            <div className="mx-auto max-w-6xl space-y-12">
                <div className="space-y-4 text-center">
                    <h1 className="text-4xl font-bold leading-tight sm:text-5xl">
                        Trở thành DashiFan của
                        <br />
                        {seriesInfo?.title ?? "Đang tải..."}
                    </h1>
                    <p className="text-xl text-gray-300">
                        Hỗ trợ trực tiếp các tác giả và dịch giả, và nhận phần thưởng cho điều đó!
                    </p>
                    <h2 className="text-3xl font-bold text-blue-400 sm:text-4xl">Không còn phải chờ đợi!</h2>
                </div>

                <div className="flex flex-wrap justify-center gap-2">
                    <Card className="w-36 overflow-hidden border-gray-700 bg-gray-800 sm:w-40">
                        <CardHeader className="bg-blue-600 text-white">
                            <CardTitle className="text-center text-sm sm:text-base">MIỄN PHÍ</CardTitle>
                        </CardHeader>
                        <CardContent className="pt-4 text-center">
                            <h3 className="text-2xl font-bold sm:text-3xl">TẤT CẢ</h3>
                            <p className="text-xs text-gray-300 sm:text-sm">
                                Các chương
                                <br />
                                đã xuất bản
                            </p>
                        </CardContent>
                    </Card>
                    <div className="flex items-center">
                        <div className="z-10 -m-6 hidden h-12 w-12 items-center justify-center rounded-full bg-white text-2xl font-bold text-gray-900 sm:flex">
                            +
                        </div>
                    </div>
                    <Card className="w-36 overflow-hidden border-gray-700 bg-gray-800 sm:w-40">
                        <CardHeader className="bg-blue-600 text-white">
                            <CardTitle className="text-center text-sm sm:text-base">Truy Cập Sớm</CardTitle>
                        </CardHeader>
                        <CardContent className="pt-4 text-center">
                            <p className="text-xs text-gray-300 sm:text-sm">lên đến</p>
                            <h3 className="text-2xl font-bold sm:text-3xl">{bestTier}</h3>
                            <p className="text-xs text-gray-300 sm:text-sm">
                                Các chương
                                <br />
                                truy cập trước
                            </p>
                        </CardContent>
                    </Card>
                </div>

                <Card
                    className={cx("border-gray-700 bg-gray-800", {
                        hidden: (currentTier && currentTier?.status !== 1) || !pending,
                    })}
                >
                    <CardContent className="space-y-4 bg-gradient-to-br from-gray-700 to-gray-800 p-6">
                        <h2 className="text-center text-2xl font-bold">Đơn hàng của bạn đang được xử lý!</h2>

                        <p className="text-center text-gray-300">
                            Chúng tôi sẽ thông báo cho bạn khi đơn hàng của bạn được xử lý xong.
                        </p>
                        <div className="flex justify-center space-x-4">
                            <Button className="bg-blue-600 text-white hover:bg-blue-700" asChild>
                                <Link href={"/account/subscriptions"}>Quản Lý Đăng Ký</Link>
                            </Button>
                        </div>
                    </CardContent>
                </Card>

                <Card
                    className={cx("border-gray-700 bg-gray-800", {
                        hidden: !isChangeTier,
                    })}
                >
                    <CardContent className="space-y-4 bg-gradient-to-br from-gray-700 to-gray-800 p-6">
                        <h2 className="text-center text-2xl font-bold">Chúc mừng, Bạn đã trở thành DashiFan!</h2>
                        <p className="text-center text-gray-300">
                            Bạn hiện đang đăng ký gói{" "}
                            <span className="font-semibold text-blue-400">{currentTier?.tier?.name}</span>.
                        </p>
                        <p className="text-center text-gray-300">
                            Bạn có quyền truy cập tất cả các chương đã xuất bản cùng {currentTier?.tier?.perks} chương
                            truy cập trước.
                        </p>
                        <div className="flex justify-center space-x-4">
                            <Button className="bg-blue-600 text-white hover:bg-blue-700" asChild>
                                <Link href={"/account/subscriptions"}>Quản Lý Đăng Ký</Link>
                            </Button>
                        </div>
                    </CardContent>
                </Card>
                <p className="text-center italic text-muted-foreground">
                    *Tất cả các thanh toán sẽ được giao dịch bằng USD!
                </p>
                <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
                    {tiers.map((tier) => (
                        <Card
                            key={tier.name}
                            className="overflow-hidden border-gray-700 bg-gray-800 transition-all duration-300 hover:-translate-y-1 hover:shadow-lg hover:shadow-blue-500/20"
                        >
                            <CardHeader className="bg-gradient-to-br from-gray-700 to-gray-800 pb-6">
                                <div className="flex items-center justify-between">
                                    <CardTitle className="text-2xl font-bold">{tier.name}</CardTitle>
                                    <Zap className="h-6 w-6 text-indigo-400" />
                                </div>
                                <CardDescription className="mt-2 text-gray-300">
                                    Mở khóa sức mạnh của truy cập sớm
                                </CardDescription>
                            </CardHeader>
                            <CardContent className="space-y-4 p-6">
                                <div>
                                    <div className="flex items-baseline justify-center">
                                        <span className="text-4xl font-extrabold text-blue-400">
                                            {tier.price.amount}đ
                                        </span>
                                        <span className="ml-1 text-xl text-gray-300">/ tháng</span>
                                    </div>
                                    <div>
                                        <p className="text-center text-sm font-extrabold text-muted-foreground">
                                            ${(tier.price.amount / 25275).toFixed(2)} / tháng
                                        </p>
                                    </div>
                                </div>
                                <Badge variant="secondary" className="w-full justify-center py-1">
                                    {tier.perks} Chương Truy Cập Trước
                                </Badge>
                                <ul className="space-y-2">
                                    <li className="flex items-center">
                                        <CheckCircle className="mr-2 h-5 w-5 flex-[0_0_28px] self-start text-green-400" />
                                        <span className="text-sm text-gray-300">
                                            Đọc {tier.perks} chương truyện trước ngày xuất bản
                                        </span>
                                    </li>
                                    <li className="flex items-center">
                                        <CheckCircle className="mr-2 h-5 w-5 flex-[0_0_28px] self-start text-green-400" />
                                        <span className="text-sm text-gray-300">
                                            Truy cập tất cả các chương đã xuất bản miễn phí
                                        </span>
                                    </li>
                                </ul>
                                {!isChangeTier ? (
                                    <Button
                                        className="w-full bg-blue-600 text-white transition-colors duration-300 hover:bg-blue-700"
                                        onClick={() => handleSubscribe(tier)}
                                    >
                                        ĐĂNG KÝ NGAY
                                    </Button>
                                ) : tier.id === currentTier.tier.id ? (
                                    <Button
                                        disabled
                                        className="w-full bg-blue-600 text-white transition-colors duration-300 hover:bg-blue-700"
                                    >
                                        ĐÃ ĐĂNG KÝ
                                    </Button>
                                ) : tier.price.amount > currentTier.tier.price.amount ? (
                                    <UpgradeDashiFan
                                        tierId={tier.id}
                                        seriesId={seriesId}
                                        subscriptionId={currentTier.subscriptionId}
                                    >
                                        <Button className="w-full bg-blue-600 text-white transition-colors duration-300 hover:bg-blue-700">
                                            NÂNG CẤP
                                        </Button>
                                    </UpgradeDashiFan>
                                ) : (
                                    <DowngradeDashiFan
                                        seriesId={seriesId}
                                        tierId={tier.id}
                                        subscriptionId={currentTier.subscriptionId}
                                    >
                                        <Button className="w-full bg-blue-600 text-white transition-colors duration-300 hover:bg-blue-700">
                                            HẠ CẤP
                                        </Button>
                                    </DowngradeDashiFan>
                                )}
                            </CardContent>
                        </Card>
                    ))}

                    {tiers.length === 0 && (
                        <div className="col-span-full text-center">
                            <p>Không có gói DashiFan nào cho truyện này. Vui lòng quay lại sau!</p>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}
