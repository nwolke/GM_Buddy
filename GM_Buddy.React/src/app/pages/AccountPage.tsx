import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Header } from "@/app/components/Header";
import { Button } from "@/app/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/app/components/ui/card";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/app/components/ui/alert-dialog";
import { useAuth } from "@/contexts/AuthContext";
import { accountApi } from "@/services/api";
import { LogOut, Trash2, Mail, ArrowLeft, Download } from "lucide-react";

export function AccountPage() {
const { user, logout } = useAuth();
const navigate = useNavigate();
const [showDeleteDialog, setShowDeleteDialog] = useState(false);
const [isDeleting, setIsDeleting] = useState(false);
const [isExporting, setIsExporting] = useState(false);

const handleLogout = () => {
  logout();
  navigate("/");
};

  const handleExportData = async () => {
    setIsExporting(true);
    try {
      const response = await accountApi.exportData();
      
      // Create a formatted JSON blob
      const jsonString = JSON.stringify(response.data, null, 2);
      const blob = new Blob([jsonString], { type: 'application/json' });
      
      // Create download link
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      
      // Generate filename with timestamp
      const timestamp = new Date().toISOString().split('T')[0];
      link.download = `gm-buddy-data-export-${timestamp}.json`;
      
      // Trigger download
      document.body.appendChild(link);
      link.click();
      
      // Cleanup
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
      
      console.log('Data exported successfully');
    } catch (error) {
      console.error('Error exporting data:', error);
      alert('Failed to export data. Please try again.');
    } finally {
      setIsExporting(false);
    }
  };

  const handleDeleteAccount = async () => {
    setIsDeleting(true);
    try {
      await accountApi.deleteAccount();

      // After successful deletion, log out and redirect
      logout();
      navigate("/");
    } catch (error) {
      console.error('Error deleting account:', error);
      alert('Failed to delete account. Please try again.');
    } finally {
      setIsDeleting(false);
      setShowDeleteDialog(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-background to-secondary/20">
      {/* Decorative background pattern */}
      <div className="fixed inset-0 opacity-5 pointer-events-none">
        <div
          className="absolute inset-0"
          style={{
            backgroundImage: `radial-gradient(circle at 2px 2px, currentColor 1px, transparent 0)`,
            backgroundSize: "40px 40px",
          }}
        />
      </div>

      <div className="container mx-auto py-8 px-4 relative">
        <Header />

        <div className="mt-8 space-y-6 max-w-4xl mx-auto">
          <div className="flex items-center gap-4">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => navigate('/')}
              className="gap-2"
            >
              <ArrowLeft className="size-4" />
              Back to Home
            </Button>
          </div>
          
          <div>
            <h2 className="text-3xl font-bold">Account Settings</h2>
            <p className="text-muted-foreground mt-2">
              Manage your account preferences and data
            </p>
          </div>

          {/* Email Display */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Mail className="size-5" />
                Email Address
              </CardTitle>
              <CardDescription>
                Your registered email address
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="flex items-center justify-between">
                <p className="text-lg font-medium">{user?.email}</p>
                {/* Future: Add change email button here */}
              </div>
            </CardContent>
          </Card>

          {/* Export Data */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Download className="size-5" />
                Download My Data
              </CardTitle>
              <CardDescription>
                Export all your campaigns, NPCs, and relationships as JSON
              </CardDescription>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-muted-foreground mb-4">
                Download a complete backup of your data in JSON format. Useful for backup purposes or before deleting your account.
              </p>
              <Button
                variant="outline"
                onClick={handleExportData}
                disabled={isExporting}
                className="w-full sm:w-auto"
              >
                <Download className="size-4 mr-2" />
                {isExporting ? "Exporting..." : "Download Data"}
              </Button>
            </CardContent>
          </Card>

          {/* Logout */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <LogOut className="size-5" />
                Sign Out
              </CardTitle>
              <CardDescription>
                Sign out of your account on this device
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Button
                variant="outline"
                onClick={handleLogout}
                className="w-full sm:w-auto"
              >
                <LogOut className="size-4 mr-2" />
                Logout
              </Button>
            </CardContent>
          </Card>

          {/* Delete Account */}
          <Card className="border-destructive/50">
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-destructive">
                <Trash2 className="size-5" />
                Delete Account
              </CardTitle>
              <CardDescription>
                Permanently delete your account and all associated data
              </CardDescription>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-muted-foreground mb-4">
                This action cannot be undone. All your campaigns, NPCs, and relationships will be permanently deleted.
              </p>
              <Button
                variant="destructive"
                onClick={() => setShowDeleteDialog(true)}
                className="w-full sm:w-auto"
              >
                <Trash2 className="size-4 mr-2" />
                Delete Account
              </Button>
            </CardContent>
          </Card>
        </div>

        {/* Delete Confirmation Dialog */}
        <AlertDialog open={showDeleteDialog} onOpenChange={setShowDeleteDialog}>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
              <AlertDialogDescription className="space-y-2">
                <p>
                  This will permanently delete your account and remove all your data from our servers.
                </p>
                <p className="font-semibold">
                  The following will be deleted:
                </p>
                <ul className="list-disc list-inside pl-2 space-y-1">
                  <li>All NPCs and player characters</li>
                  <li>All campaigns</li>
                  <li>All relationships and connections</li>
                  <li>All other associated data</li>
                </ul>
                <p className="text-destructive font-semibold mt-4">
                  This action cannot be undone.
                </p>
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel disabled={isDeleting}>
                Cancel
              </AlertDialogCancel>
              <AlertDialogAction
                onClick={handleDeleteAccount}
                disabled={isDeleting}
                className="bg-destructive hover:bg-destructive/90"
              >
                {isDeleting ? "Deleting..." : "Delete Account"}
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      </div>
    </div>
  );
}
