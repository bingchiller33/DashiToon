"use client";

import AdminSettingLayout from "@/components/AdminSettingLayout";
import SiteLayout from "@/components/SiteLayout";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import {
    createKanaPack,
    updateMission,
    getKanaPacks,
    getMissions,
    KanaPackItem,
    updateKanaPack,
    createMission,
    Mission,
} from "@/utils/api/admin";
import { Plus } from "lucide-react";
import React, { useEffect, useState } from "react";
import { toast } from "sonner";
import { KanaPacksTable } from "../_components/KanaPacksTable";
import { MissionsTable } from "../_components/MissionTable";
import { RatesForm } from "../_components/RatesForm";
import { ROLES } from "@/utils/consts";

export default function ManageKanaScreen(): React.ReactElement {
    const [kanaPacksLoading, setKanaPacksLoading] = useState<boolean>(true);
    const [kanaPacks, setKanaPacks] = useState<KanaPackItem[]>([]);
    const [missionsLoading, setMissionsLoading] = useState<boolean>(true);
    const [missions, setMissions] = useState<Mission[]>([]);
    const [isKanaPackDialogOpen, setIsKanaPackDialogOpen] = useState<boolean>(false);
    const [isMissionDialogOpen, setIsMissionDialogOpen] = useState<boolean>(false);
    const [editingKanaPack, setEditingKanaPack] = useState<KanaPackItem | null>(null);
    const [editingMission, setEditingMission] = useState<Mission | null>(null);

    useEffect(() => {
        async function fetchKanaPacks() {
            const [packs, err] = await getKanaPacks();
            if (err) {
                toast.error("Không thể tải gói KanaGold");
                return;
            }

            setKanaPacks(packs);
            setKanaPacksLoading(false);
        }
        fetchKanaPacks();

        async function fetchMissions() {
            const [missions, err] = await getMissions();
            if (err) {
                toast.error("Không thể tải nhiệm vụ KanaCoin");
                return;
            }

            setMissions(missions);
            setMissionsLoading(false);
        }

        fetchMissions();
    }, []);

    const handleKanaPackSubmit = async (data: Omit<KanaPackItem, "id" | "isActive">) => {
        if (data.amount <= 0 || data.price.amount <= 0) {
            toast.error("Số lượng và giá phải lớn hơn 0");
            return;
        }

        try {
            if (editingKanaPack) {
                const [res, err] = await updateKanaPack(
                    editingKanaPack.id,
                    data.amount,
                    data.price.amount,
                    editingKanaPack.isActive,
                );

                if (err) {
                    toast.error("Không thể cập nhật gói KanaGold");
                    return;
                }

                toast.success("Cập nhật gói KanaGold thành công");
                window.location.reload();
            } else {
                const [res, err] = await createKanaPack(data.amount, data.price.amount);

                if (err) {
                    toast.error("Không thể tạo gói KanaGold");
                    return;
                }

                toast.success("Tạo gói KanaGold thành công");
                window.location.reload();
            }
        } finally {
            setIsKanaPackDialogOpen(false);
            setEditingKanaPack(null);
        }
    };

    const handleMissionSubmit = async (data: Omit<Mission, "id" | "isActive">) => {
        try {
            if (editingMission) {
                const [res, err] = await updateMission(
                    editingMission.id,
                    data.readCount,
                    data.reward,
                    editingMission.isActive,
                );

                if (err) {
                    toast.error("Không thể cập nhật nhiệm vụ KanaCoin");
                    return;
                }

                toast.success("Cập nhật nhiệm vụ thành công");
                window.location.reload();
            } else {
                const [res, err] = await createMission(data.readCount, data.reward);

                if (err) {
                    toast.error("Không thể tạo gói KanaGold");
                    return;
                }

                toast.success("Tạo gói KanaGold thành công");
                window.location.reload();
            }
        } finally {
            setIsMissionDialogOpen(false);
            setEditingMission(null);
        }
    };

    return (
        <SiteLayout allowedRoles={[ROLES.Administrator]} hiddenUntilLoaded>
            <div className="container pt-6">
                <AdminSettingLayout>
                    <div className="space-y-8 p-6 text-neutral-100">
                        <h1 className="mb-6 text-3xl font-bold">Quản lý Kana</h1>

                        <section>
                            <RatesForm />
                        </section>

                        <section>
                            <div className="mb-4 flex items-center justify-between">
                                <h2 className="text-2xl font-semibold">Gói KanaGold</h2>
                                <Dialog open={isKanaPackDialogOpen} onOpenChange={setIsKanaPackDialogOpen}>
                                    <DialogTrigger asChild>
                                        <Button onClick={() => setEditingKanaPack(null)}>
                                            <Plus className="mr-2 h-4 w-4" />
                                            Tạo gói mới
                                        </Button>
                                    </DialogTrigger>
                                    <DialogContent>
                                        <DialogHeader>
                                            <DialogTitle>
                                                {editingKanaPack ? "Chỉnh sửa gói KanaGold" : "Tạo gói KanaGold mới"}
                                            </DialogTitle>
                                        </DialogHeader>
                                        <KanaPackForm onSubmit={handleKanaPackSubmit} initialData={editingKanaPack} />
                                    </DialogContent>
                                </Dialog>
                            </div>
                            {kanaPacksLoading ? (
                                <KanaPacksTableSkeleton />
                            ) : (
                                <KanaPacksTable
                                    data={kanaPacks}
                                    onEdit={(pack) => {
                                        setEditingKanaPack(pack);
                                        setIsKanaPackDialogOpen(true);
                                    }}
                                    onActiveChanged={async (pack, isActive) => {
                                        const [res, err] = await updateKanaPack(
                                            pack.id,
                                            pack.amount,
                                            pack.price.amount,
                                            isActive,
                                        );

                                        if (err) {
                                            toast.error("Không thể cập nhật gói");
                                            return;
                                        }

                                        toast.success("Cập nhật gói thành công");

                                        setKanaPacks((prev) =>
                                            prev.map((p) =>
                                                p.id === pack.id
                                                    ? {
                                                          ...p,
                                                          isActive,
                                                      }
                                                    : p,
                                            ),
                                        );
                                    }}
                                />
                            )}
                        </section>

                        <section>
                            <div className="mb-4 flex items-center justify-between">
                                <h2 className="text-2xl font-semibold">Nhiệm vụ KanaCoin</h2>
                                <Dialog open={isMissionDialogOpen} onOpenChange={setIsMissionDialogOpen}>
                                    <DialogTrigger asChild>
                                        <Button onClick={() => setEditingMission(null)}>
                                            <Plus className="mr-2 h-4 w-4" />
                                            Tạo nhiệm vụ mới
                                        </Button>
                                    </DialogTrigger>
                                    <DialogContent>
                                        <DialogHeader>
                                            <DialogTitle>
                                                {editingMission ? "Chỉnh sửa nhiệm vụ" : "Tạo nhiệm vụ mới"}
                                            </DialogTitle>
                                        </DialogHeader>
                                        <MissionForm onSubmit={handleMissionSubmit} initialData={editingMission} />
                                    </DialogContent>
                                </Dialog>
                            </div>
                            {missionsLoading ? (
                                <MissionsTableSkeleton />
                            ) : (
                                <MissionsTable
                                    data={missions}
                                    onEdit={(mission) => {
                                        setEditingMission(mission);
                                        setIsMissionDialogOpen(true);
                                    }}
                                    onActiveChanged={async (mission, isActive) => {
                                        const [res, err] = await updateMission(
                                            mission.id,
                                            mission.readCount,
                                            mission.reward,
                                            isActive,
                                        );

                                        if (err) {
                                            toast.error("Không thể cập nhật gói");
                                            return;
                                        }

                                        toast.success("Cập nhật gói thành công");

                                        setMissions((prev) =>
                                            prev.map((m) => (m.id === mission.id ? { ...m, isActive } : m)),
                                        );
                                    }}
                                />
                            )}
                        </section>
                    </div>
                </AdminSettingLayout>
            </div>
        </SiteLayout>
    );
}

interface KanaPackFormProps {
    onSubmit: (data: Omit<KanaPackItem, "id" | "isActive">) => void;
    initialData: KanaPackItem | null;
}

function KanaPackForm({ onSubmit, initialData }: KanaPackFormProps): React.ReactElement {
    const [amount, setAmount] = useState<number>(initialData?.amount || 0);
    const [price, setPrice] = useState<number>(initialData?.price.amount || 0);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>): void => {
        e.preventDefault();
        onSubmit({ amount, price: { currency: "VND", amount: price } });
    };

    return (
        <form onSubmit={handleSubmit} className="space-y-4">
            <div>
                <Label htmlFor="amount">Số lượng KanaGold</Label>
                <Input
                    id="amount"
                    type="number"
                    value={amount}
                    onChange={(e) => setAmount(Number(e.target.value))}
                    required
                />
            </div>
            <div>
                <Label htmlFor="price">Giá (VND)</Label>
                <Input
                    id="price"
                    type="number"
                    value={price}
                    onChange={(e) => setPrice(Number(e.target.value))}
                    required
                />
            </div>
            <div className="flex justify-end space-x-2">
                <Button
                    type="button"
                    variant="outline"
                    onClick={() =>
                        onSubmit({
                            amount: 0,
                            price: { currency: "VND", amount: 0 },
                        })
                    }
                >
                    Hủy
                </Button>
                <Button type="submit">Lưu</Button>
            </div>
        </form>
    );
}

interface MissionFormProps {
    onSubmit: (data: Omit<Mission, "id" | "isActive">) => void;
    initialData: Mission | null;
}

function MissionForm({ onSubmit, initialData }: MissionFormProps): React.ReactElement {
    const [readCount, setReadCount] = useState<number>(initialData?.readCount || 0);
    const [reward, setReward] = useState<number>(initialData?.reward || 0);

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>): void => {
        e.preventDefault();
        onSubmit({ readCount, reward });
    };

    return (
        <form onSubmit={handleSubmit} className="space-y-4">
            <div>
                <Label htmlFor="readCount">Số chương cần đọc</Label>
                <Input
                    id="readCount"
                    type="number"
                    value={readCount}
                    onChange={(e) => setReadCount(Number(e.target.value))}
                    required
                />
            </div>
            <div>
                <Label htmlFor="reward">Phần thưởng (KanaCoin)</Label>
                <Input
                    id="reward"
                    type="number"
                    value={reward}
                    onChange={(e) => setReward(Number(e.target.value))}
                    required
                />
            </div>
            <div className="flex justify-end space-x-2">
                <Button type="button" variant="outline" onClick={() => onSubmit({ readCount: 0, reward: 0 })}>
                    Hủy
                </Button>
                <Button type="submit">Lưu</Button>
            </div>
        </form>
    );
}

function KanaPacksTableSkeleton(): React.ReactElement {
    return (
        <div className="space-y-2">
            {[...Array(5)].map((_, i) => (
                <div key={i} className="flex space-x-4">
                    <Skeleton className="h-4 w-1/4" />
                    <Skeleton className="h-4 w-1/4" />
                    <Skeleton className="h-4 w-1/4" />
                    <Skeleton className="h-4 w-1/4" />
                </div>
            ))}
        </div>
    );
}

function MissionsTableSkeleton(): React.ReactElement {
    return (
        <div className="space-y-2">
            {[...Array(5)].map((_, i) => (
                <div key={i} className="flex space-x-4">
                    <Skeleton className="h-4 w-1/5" />
                    <Skeleton className="h-4 w-1/5" />
                    <Skeleton className="h-4 w-1/5" />
                    <Skeleton className="h-4 w-1/5" />
                    <Skeleton className="h-4 w-1/5" />
                </div>
            ))}
        </div>
    );
}
