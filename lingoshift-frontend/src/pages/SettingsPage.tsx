import React, { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
    Box,
    Typography,
    Button,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Paper,
    IconButton,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    TextField,
    Select,
    MenuItem,
    FormControlLabel,
    Checkbox,
} from '@mui/material';
import { Edit as EditIcon, Delete as DeleteIcon, Add as AddIcon } from '@mui/icons-material';
import { getSequences, addSequence, updateSequence, removeSequence, getAvailableActions, getAvailableLanguages } from '../services/api';
import { SequenceConfig, SequenceAction, Language } from '../types';

const SettingsPage = () => {
    const queryClient = useQueryClient();
    const [open, setOpen] = useState(false);
    const [editingSequence, setEditingSequence] = useState<SequenceConfig | null>(null);

    const { data: sequences = [] } = useQuery<SequenceConfig[]>({ queryKey: ['sequences'], queryFn: getSequences });
    const { data: availableActions = [] } = useQuery<SequenceAction[]>({ queryKey: ['availableActions'], queryFn: getAvailableActions });
    const { data: availableLanguages = [] } = useQuery<Language[]>({ queryKey: ['availableLanguages'], queryFn: getAvailableLanguages });

    const addMutation = useMutation({
        mutationFn: addSequence,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['sequences'] });
            handleClose();
        },
    });

    const updateMutation = useMutation({
        mutationFn: updateSequence,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['sequences'] });
            handleClose();
        },
    });

    const removeMutation = useMutation({
        mutationFn: removeSequence,
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['sequences'] }),
    });

    const handleOpen = (sequence: SequenceConfig | null = null) => {
        setEditingSequence(sequence || {
            sequenceName: '',
            sequence: '',
            action: availableActions[0]?.value || '',
            targetLanguage: availableLanguages[0]?.code || '',
            useLLM: false,
            showPopup: false,
        });
        setOpen(true);
    };

    const handleClose = () => {
        setOpen(false);
        setEditingSequence(null);
    };

    const handleSave = () => {
        if (editingSequence) {
            if ('id' in editingSequence) {
                updateMutation.mutate(editingSequence);
            } else {
                addMutation.mutate(editingSequence);
            }
        }
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement> | { target: { name: string; value: unknown } }) => {
        const { name, value, checked } = e.target as HTMLInputElement;
        setEditingSequence(prev => {
            if (!prev) return null;
            if (name === 'action') {
                return { ...prev, action: { value: value as string } };
            }
            if (name === 'targetLanguage') {
                const selectedLanguage = availableLanguages.find(lang => lang.code === value);
                return { ...prev, targetLanguage: selectedLanguage || { code: value as string, name: '' } };
            }
            return {
                ...prev,
                [name]: name === 'useLLM' || name === 'showPopup' ? checked : value
            };
        });
    };

    return (
        <Box sx={{ maxWidth: 1200, margin: 'auto', padding: 2 }}>
            <Typography variant="h4" gutterBottom>Sequence Settings</Typography>
            <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={() => handleOpen()}
                sx={{ marginBottom: 2 }}
            >
                Add New Sequence
            </Button>
            <TableContainer component={Paper}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>Name</TableCell>
                            <TableCell>Sequence</TableCell>
                            <TableCell>Action</TableCell>
                            <TableCell>Target Language</TableCell>
                            <TableCell>Use LLM</TableCell>
                            <TableCell>Show Popup</TableCell>
                            <TableCell>Actions</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {sequences.map((seq, index) => (
                            <TableRow key={seq.id || index}>
                                <TableCell>{seq.sequenceName}</TableCell>
                                <TableCell>{seq.sequence}</TableCell>
                                <TableCell>{typeof seq.action === 'object' ? seq.action.value : seq.action}</TableCell>
                                <TableCell>{typeof seq.targetLanguage === 'object' ? seq.targetLanguage.name : seq.targetLanguage}</TableCell>
                                <TableCell>{seq.useLLM ? 'Yes' : 'No'}</TableCell>
                                <TableCell>{seq.showPopup ? 'Yes' : 'No'}</TableCell>
                                <TableCell>
                                    <IconButton onClick={() => handleOpen(seq)}>
                                        <EditIcon />
                                    </IconButton>
                                    <IconButton onClick={() => 'id' in seq && removeMutation.mutate(seq.id)}>
                                        <DeleteIcon />
                                    </IconButton>
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>

            <Dialog open={open} onClose={handleClose}>
                <DialogTitle>{editingSequence && 'id' in editingSequence ? 'Edit Sequence' : 'Add New Sequence'}</DialogTitle>
                <DialogContent>
                    <TextField
                        autoFocus
                        margin="dense"
                        name="sequenceName"
                        label="Sequence Name"
                        fullWidth
                        value={editingSequence?.sequenceName || ''}
                        onChange={handleChange}
                    />
                    <TextField
                        margin="dense"
                        name="sequence"
                        label="Sequence"
                        fullWidth
                        value={editingSequence?.sequence || ''}
                        onChange={handleChange}
                    />
                    <Select
                        fullWidth
                        margin="dense"
                        name="action"
                        value={editingSequence?.action?.value || ''}
                        onChange={handleChange}
                    >
                        {availableActions.map((action) => (
                            <MenuItem key={action.value} value={action.value}>
                                {action.value}
                            </MenuItem>
                        ))}
                    </Select>
                    <Select
                        fullWidth
                        margin="dense"
                        name="targetLanguage"
                        value={editingSequence?.targetLanguage?.code || ''}
                        onChange={handleChange}
                    >
                        {availableLanguages.map((lang) => (
                            <MenuItem key={lang.code} value={lang.code}>
                                {lang.name}
                            </MenuItem>
                        ))}
                    </Select>
                    <FormControlLabel
                        control={
                            <Checkbox
                                checked={editingSequence?.useLLM || false}
                                onChange={handleChange}
                                name="useLLM"
                            />
                        }
                        label="Use LLM"
                    />
                    <FormControlLabel
                        control={
                            <Checkbox
                                checked={editingSequence?.showPopup || false}
                                onChange={handleChange}
                                name="showPopup"
                            />
                        }
                        label="Show Popup"
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleClose}>Cancel</Button>
                    <Button onClick={handleSave}>Save</Button>
                </DialogActions>
            </Dialog>
        </Box>
    );
};

export default SettingsPage;