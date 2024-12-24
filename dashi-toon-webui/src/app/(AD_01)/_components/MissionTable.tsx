import { Button } from "@/components/ui/button";
import { Switch } from "@/components/ui/switch";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import { Mission } from "@/utils/api/admin";
import { Edit } from "lucide-react";

export interface MissionsTableProps {
    data: Mission[];
    onEdit: (mission: Mission) => void;
    onActiveChanged: (mission: Mission, isActive: boolean) => void;
}

export function MissionsTable({
    data,
    onEdit,
    onActiveChanged,
}: MissionsTableProps): React.ReactElement {
    return (
        <Table>
            <TableHeader>
                <TableRow>
                    <TableHead>ID</TableHead>
                    <TableHead>Số chương cần đọc</TableHead>
                    <TableHead>Phần thưởng (KanaCoin)</TableHead>
                    <TableHead>Trạng thái</TableHead>
                    <TableHead>Hành động</TableHead>
                </TableRow>
            </TableHeader>
            <TableBody>
                {data.map((mission) => (
                    <TableRow key={mission.id}>
                        <TableCell>{mission.id}</TableCell>
                        <TableCell>{mission.readCount}</TableCell>
                        <TableCell>{mission.reward}</TableCell>
                        <TableCell>
                            <Switch
                                checked={mission.isActive}
                                onCheckedChange={(e) =>
                                    onActiveChanged(mission, e)
                                }
                            />
                        </TableCell>
                        <TableCell>
                            <Button
                                variant="outline"
                                size="sm"
                                onClick={() => onEdit(mission)}
                            >
                                <Edit className="mr-2 h-4 w-4" />
                                Chỉnh sửa
                            </Button>
                        </TableCell>
                    </TableRow>
                ))}
            </TableBody>
        </Table>
    );
}
