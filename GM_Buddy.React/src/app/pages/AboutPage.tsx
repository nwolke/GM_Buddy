import { Header } from "@/app/components/Header";
import { Button } from "@/app/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/app/components/ui/card";
import { ArrowLeft, Mail, Scale, Code } from "lucide-react";
import { useNavigate } from "react-router-dom";

export function AboutPage() {
  const navigate = useNavigate();

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
            <h2 className="text-3xl font-bold">About GM Buddy</h2>
            <p className="text-muted-foreground mt-2">
              Information about this project
            </p>
          </div>

          {/* Creator Information */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Code className="size-5" />
                About the Creator
              </CardTitle>
              <CardDescription>
                Who built this application
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <p className="text-sm text-muted-foreground">
                Content will be provided by the site creator.
              </p>
            </CardContent>
          </Card>

          {/* Contact Information */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Mail className="size-5" />
                Contact Information
              </CardTitle>
              <CardDescription>
                Get in touch
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <p className="text-sm text-muted-foreground">
                Contact details to be determined.
              </p>
            </CardContent>
          </Card>

          {/* Legal Information */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Scale className="size-5" />
                Legal Information
              </CardTitle>
              <CardDescription>
                D&D SRD and legal declarations
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-3">
                <h3 className="font-semibold text-sm">System Reference Document</h3>
                <p className="text-sm text-muted-foreground">
                  This application uses content from the Dungeons &amp; Dragons 5th Edition System Reference Document (SRD).
                </p>
                <p className="text-sm text-muted-foreground">
                  D&D SRD content and associated images will be properly attributed here.
                </p>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
