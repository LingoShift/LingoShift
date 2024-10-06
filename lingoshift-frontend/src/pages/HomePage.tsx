import React from 'react';
import { Typography, Box, Paper, Grid, Button } from '@mui/material';
import TranslateIcon from '@mui/icons-material/Translate';
import SettingsIcon from '@mui/icons-material/Settings';
import { Link } from 'react-router-dom';

const HomePage = () => {
    return (
        <Box sx={{ flexGrow: 1 }}>
            <Typography variant="h4" gutterBottom>
                Welcome to LingoShift
            </Typography>
            <Typography variant="subtitle1" paragraph>
                Your intelligent translation and language processing assistant.
            </Typography>
            <Grid container spacing={3}>
                <Grid item xs={12} md={6}>
                    <Paper elevation={3} sx={{ p: 3 }}>
                        <Typography variant="h6" gutterBottom>
                            Quick Translation
                        </Typography>
                        <Typography paragraph>
                            Start translating text instantly with our advanced AI-powered translation engine.
                        </Typography>
                        <Button
                            variant="contained"
                            startIcon={<TranslateIcon />}
                            component={Link}
                            to="/translate"
                        >
                            Start Translating
                        </Button>
                    </Paper>
                </Grid>
                <Grid item xs={12} md={6}>
                    <Paper elevation={3} sx={{ p: 3 }}>
                        <Typography variant="h6" gutterBottom>
                            Configure Settings
                        </Typography>
                        <Typography paragraph>
                            Customize LingoShift to fit your needs. Set up sequence configurations and more.
                        </Typography>
                        <Button
                            variant="outlined"
                            startIcon={<SettingsIcon />}
                            component={Link}
                            to="/settings"
                        >
                            Go to Settings
                        </Button>
                    </Paper>
                </Grid>
            </Grid>
        </Box>
    );
};

export default HomePage;