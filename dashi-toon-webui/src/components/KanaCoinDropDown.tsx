import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";
import Link from "next/link";
import { UserSession } from "@/utils/api/user";
import { useEffect, useState } from "react";
import {
    checkIn,
    getUserKanas,
    getUserMetadata,
    UserKanas,
    UserMetadata,
    Mission,
    getUserMissions,
    claimMissionReward,
} from "@/utils/api/reader/kana";
import { CoinsIcon, Fan } from "lucide-react";
import { toast } from "sonner";
import { MdHistory } from "react-icons/md";
import { FaQuestion } from "react-icons/fa";
import { CiCoins1 } from "react-icons/ci";
import { Progress } from "@/components/ui/progress";
import { RiCalendarCheckLine } from "react-icons/ri";
import { GoGoal } from "react-icons/go";

interface KanaCoinDropDownProps {
    session: UserSession | null;
}

export default function KanaCoinDropDown({ session }: KanaCoinDropDownProps) {
    const [userKanas, setUserKanas] = useState<UserKanas | null>(null);
    const [userMetadata, setUserMetadata] = useState<UserMetadata | null>(null);
    const [missions, setMissions] = useState<Mission[]>([]);

    useEffect(() => {
        async function fetchData() {
            if (!session) return;
            const [kanasData, _] = await getUserKanas();
            const [metaData, __] = await getUserMetadata();
            const [missionsData, ___] = await getUserMissions();

            if (kanasData) setUserKanas(kanasData);
            if (metaData) setUserMetadata(metaData);
            if (missionsData) setMissions(missionsData);
        }
        fetchData();
    }, [session]);

    const handleCheckIn = async () => {
        const [_, error] = await checkIn();
        if (error) {
            toast.error("Không thể điểm danh. Vui lòng thử lại sau.");
        } else {
            toast.success("Điểm danh thành công!");
            refreshData();
        }
    };

    const handleClaimReward = async (missionId: string) => {
        const [data, error] = await claimMissionReward(missionId);
        if (error) {
            toast.error("Không thể nhận thưởng", error);
        } else {
            toast.success("Đã nhận thưởng thành công");
            refreshData();
        }
    };

    const refreshData = async () => {
        const [kanasData, _] = await getUserKanas();
        const [metaData, __] = await getUserMetadata();
        const [missionsData, ___] = await getUserMissions();

        if (kanasData) setUserKanas(kanasData);
        if (metaData) setUserMetadata(metaData);
        if (missionsData) setMissions(missionsData);
    };

    const getKanaAmount = (kanaType: 1 | 2) => {
        return userKanas?.totals.find((total) => total.kanaType === kanaType)?.amount ?? 0;
    };

    if (!session) return null;

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button variant="ghost" className="flex items-center space-x-2 text-gray-300">
                    <div className="flex items-center space-x-1">
                        <span className="text-gray-400">
                            <CoinsIcon />
                        </span>
                        <span>{getKanaAmount(2)}</span>
                    </div>
                    <div className="flex items-center space-x-1">
                        <span className="text-yellow-400">
                            <CoinsIcon />
                        </span>
                        <span>{getKanaAmount(1)}</span>
                    </div>
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="w-96" align="end">
                <div className="p-4">
                    <div className="flex flex-col space-y-4">
                        <div className="flex flex-col space-y-1.5">
                            <div className="flex items-center justify-between">
                                <h3 className="flex items-center text-sm font-semibold">
                                    <RiCalendarCheckLine className="mr-1 h-4 w-4" />
                                    Điểm Danh
                                </h3>
                                <div className="flex items-center">
                                    <CoinsIcon className="mr-1 h-4 w-4 text-gray-400" />
                                    <p className="text-xl font-bold">
                                        100 <span className="text-sm text-muted-foreground">/ lần</span>
                                    </p>
                                </div>
                            </div>
                            <Button
                                onClick={handleCheckIn}
                                disabled={userMetadata?.isCheckedIn}
                                variant="outline"
                                className="w-full"
                            >
                                {userMetadata?.isCheckedIn ? "Đã Nhận Thưởng Hôm Nay" : "Nhận KanaCoin Hàng Ngày"}
                            </Button>
                        </div>

                        <div className="space-y-3">
                            <div className="flex items-center justify-between">
                                <h3 className="flex items-center text-sm font-semibold">
                                    <GoGoal className="mr-1 h-4 w-4" />
                                    Nhiệm Vụ
                                </h3>
                                <Link href="/account/manage-currency">
                                    <Button
                                        variant="ghost"
                                        size="sm"
                                        className="h-7 text-xs text-blue-400 hover:text-blue-500"
                                    >
                                        Xem thêm
                                    </Button>
                                </Link>
                            </div>
                            {missions.filter((mission) => !mission.isCompleted).length > 0 ? (
                                missions
                                    .filter((mission) => !mission.isCompleted)
                                    .slice(-1)
                                    .map((mission) => {
                                        const progress = Math.min(
                                            ((userMetadata?.currentDateChapterRead ?? 0) / mission.readCount) * 100,
                                            100,
                                        );
                                        return (
                                            <div
                                                key={mission.missionId}
                                                className="space-y-2 rounded-lg border border-neutral-700 bg-neutral-800/50 p-3"
                                            >
                                                <div className="flex items-center justify-between text-sm">
                                                    <span>Đọc {mission.readCount} chương mới</span>
                                                    <span className="flex items-center">
                                                        <CoinsIcon className="mr-1 h-4 w-4 text-gray-400" />
                                                        {mission.amount}
                                                    </span>
                                                </div>
                                                <Progress value={progress} className="h-1" />
                                                <div className="flex items-center justify-between text-xs">
                                                    <span className="text-gray-400">
                                                        {userMetadata?.currentDateChapterRead ?? 0}/{mission.readCount}
                                                    </span>
                                                    <Button
                                                        variant="outline"
                                                        size="sm"
                                                        disabled={!mission.isCompletable}
                                                        onClick={() => handleClaimReward(mission.missionId)}
                                                        className="h-7 text-xs"
                                                    >
                                                        Nhận thưởng
                                                    </Button>
                                                </div>
                                            </div>
                                        );
                                    })
                            ) : (
                                <div className="rounded-lg border border-neutral-700 bg-neutral-800/50 p-3 text-center text-sm text-gray-400">
                                    Bạn đã hoàn thành tất cả nhiệm vụ hôm nay!
                                </div>
                            )}
                        </div>
                    </div>
                </div>

                <DropdownMenuSeparator />
                <Link href="/account/manage-currency">
                    <DropdownMenuItem>
                        <CiCoins1 className="mr-2 h-4 w-4" />
                        Quản Lý Xu
                    </DropdownMenuItem>
                </Link>
                <Link href="/account/subscriptions">
                    <DropdownMenuItem>
                        <Fan className="mr-2 h-4 w-4" />
                        DashiFan
                    </DropdownMenuItem>
                </Link>
                <Link href="/account/payment-history">
                    <DropdownMenuItem>
                        <MdHistory className="mr-2 h-4 w-4" />
                        Lịch sử giao dịch
                    </DropdownMenuItem>
                </Link>
                <Link href="/faq">
                    <DropdownMenuItem>
                        <FaQuestion className="mr-2 h-4 w-4" />
                        FAQ
                    </DropdownMenuItem>
                </Link>
            </DropdownMenuContent>
        </DropdownMenu>
    );
}
